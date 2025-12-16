using System.Diagnostics;
using System.Text;

namespace Morph.Scanning;

/// <summary>
/// Provides functionality for scanning source code and producing a list of tokens
/// </summary>
internal static class Scanner
{
	/// <summary>
	/// Represents the result of scanning a sequence for tokens, including the number of characters scanned, the number of
	/// new lines encountered, and the tokens identified.
	/// </summary>
	/// <param name="ScanLength">The total number of characters in the source that were scanned to produce the tokens.</param>
	/// <param name="NewLines">The number of new line characters encountered during the scan.</param>
	/// <param name="Tokens">A collection of tokens identified during the scan. May contain zero or more tokens.</param>
	private record ScanTokenResult(int ScanLength, int NewLines, params IEnumerable<Token> Tokens);

	/// <summary>
	/// Scans the specified source text and returns a list of tokens representing the lexical elements found.
	/// </summary>
	/// <remarks>The returned list includes all tokens found in the source, including whitespace, comments, and an
	/// explicit EOF token at the end. The method does not modify the input string.</remarks>
	/// <param name="source">The source text to scan for tokens. Cannot be null.</param>
	/// <returns>A list of tokens identified in the source text, in the order they appear. The list always ends with an end-of-file
	/// (EOF) token.</returns>
	public static List<Token> ScanTokens(string source)
	{
		ArgumentNullException.ThrowIfNull(source);

		var tokens = new List<Token>();

		int nextTokenStart = 0;
		int currentLine = 1;

		while (nextTokenStart < source.Length)
		{
			var scanResult = ScanNextTokens(source, nextTokenStart, currentLine);

			nextTokenStart += scanResult.ScanLength;
			currentLine += scanResult.NewLines;
			tokens.AddRange(scanResult.Tokens);
		}

		tokens.Add(new Token(TokenType.Eof, "", null, currentLine));
		return tokens;
	}

	/// <summary>
	/// Scans the next token or sequence of tokens from the source code, starting at the specified position and line
	/// number.
	/// </summary>
    private static ScanTokenResult ScanNextTokens(string source, int current, int currentLine)
    {
		int start = current;
		Token token;

		char c = source[current];

		// Single character tokens
        if (Lexemes.SingleCharacterTokens.TryGetValue(c, out TokenType singleCharToken))
        {
			token = new Token(singleCharToken, c.ToString(), null, currentLine);
            return new ScanTokenResult(1, 0, token);
        }

		// Single or double character tokens
		if (Lexemes.DoubleCharacterTokenStarts.TryGetValue(c, out TokenType tokenType))
		{
			// Could still be a double character token, so check the next char
			var nextChar = Peek(source, current);
			var possibleDoubleCharToken = $"{c}{nextChar}";

			if (Lexemes.DoubleCharacterTokens.TryGetValue(possibleDoubleCharToken, out TokenType doubleCharToken))
			{
				token = new Token(doubleCharToken, possibleDoubleCharToken, null, currentLine);
				return new ScanTokenResult(2, 0, token);
			}

			token = new Token(tokenType, c.ToString(), null, currentLine);
			return new ScanTokenResult(1, 0, token);
		}

        if (c == '/')
        {
            if (Peek(source, current) != '/')
            {
				token = new Token(TokenType.Slash, "/", null, currentLine);
				return new ScanTokenResult(1, 0, token);
            }

			// Comment. So scan to the end of the line or file
			while (!IsAtEnd(source, current) && source[current] != '\n')
            {
                current++;
            }

			return new ScanTokenResult(current - start, 0);
        }

        if (c == '"')
        {
            return ScanString(source, current, currentLine);
        }

        if (c == '$')
        {
            if (Peek(source, current) == '"')
            {
                return ScanInterpolatedString(source, current, currentLine);
            }
            
			Morph.Error(currentLine, "Unexpected character after '$'");
			return new ScanTokenResult(1, 0);
        }

        if (c == '\n')
        {
            return new ScanTokenResult(1, 1);
		}

        if (char.IsWhiteSpace(c))
        {
            return new ScanTokenResult(1, 0);
        }

        if (char.IsDigit(c))
        {
            return ScanNumber(source, current, currentLine);
        }

        if (char.IsLetter(c))
        {
            return ScanIdentifier(source, current, currentLine);
        }

		// Reached an unexpected character, but fail gracefully and attempt
		// to scan the rest of the source to spot any more errors
        Morph.Error(currentLine, $"Unexpected character: {c}");
		return new ScanTokenResult(1, 0);
	}

    private static ScanTokenResult ScanString(string source, int current, int currentLine)
    {
		Debug.Assert(source[current] == '"');

		current++;
		int startOfString = current;

		var sb = new StringBuilder();
        bool escaped = false;
		int newLines = 0;

        while (!IsAtEnd(source, current))
        {
            char c = source[current];
			current++;

            if (escaped)
            {
                escaped = false;
				
				var escapedCharacter = c switch
				{
					'"' => '"',
					'\\' => '\\',
					'n' => '\n',
					'r' => '\r',
					't' => '\t',
					_ => '_'
				};

				if (escapedCharacter == '_')
				{
					Morph.Error(currentLine, $"Unable to escape character '{c}'");
					continue;
				}

				sb.Append(escapedCharacter);
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

			if (c == '\n')
			{
				newLines++;
				currentLine++;
			}

			if (c == '"')
            {
				var token = new Token(TokenType.String, sb.ToString(), null, currentLine);
				return new ScanTokenResult(current - startOfString + 1, newLines, token);
            }

            sb.Append(c);
        }

        Morph.Error(currentLine, "Unterminated string.");
        return new ScanTokenResult(sb.Length + 1, newLines);
    }

    private static ScanTokenResult ScanInterpolatedString(string source, int current, int currentLine)
    {
		Token token;
		var tokens = new List<Token>();
		var sb = new StringBuilder();
		bool escaped = false;
		int newLines = 0;
		
		Debug.Assert(source[current] == '$');
		current++;
		
		Debug.Assert(source[current] == '"');
		current++;

		int startOfString = current;

		var startToken = new Token(TokenType.InterpolatedStringStart, "$\"", null, currentLine);
		tokens.Add(startToken);

		while (!IsAtEnd(source, current))
		{
			char c = source[current];
			current++;

			if (escaped)
			{
				escaped = false;

				var escapedCharacter = c switch
				{
					'"' => '"',
					'\\' => '\\',
					'n' => '\n',
					'r' => '\r',
					't' => '\t',
					'[' => '[',
					_ => '_'
				};

				if (escapedCharacter == '_')
				{
					Morph.Error(currentLine, $"Unable to escape character '{c}'");
					continue;
				}

				sb.Append(escapedCharacter);
				continue;
			}

			if (c == '\\')
			{
				escaped = true;
				continue;
			}

			if (c == '\n')
			{
				newLines++;
				currentLine++;
			}

			if (c == '[')
			{
				// Emit any string part before the expression
				if (sb.Length > 0)
				{
					token = new Token(TokenType.String, sb.ToString(), null, currentLine);
					tokens.Add(token);
					sb.Clear();
				}

				token = new Token(TokenType.InterpolatedStringExpressionStart, sb.ToString(), null, currentLine);
				tokens.Add(token);

				while (!IsAtEnd(source, current) && source[current] != ']')
				{
					var expressionTokenResult = ScanNextTokens(source, current, currentLine);
					current += expressionTokenResult.ScanLength;
					currentLine += expressionTokenResult.NewLines;
					tokens.AddRange(expressionTokenResult.Tokens);
				}

				if (IsAtEnd(source, current))
				{
					Morph.Error(currentLine, "Unterminated interpolation in string.");
					break;
				}

				token = new Token(TokenType.InterpolatedStringExpressionEnd, sb.ToString(), null, currentLine);
				tokens.Add(token);

				current++;

				continue;
			}

			if (c == '"')
			{
				if (sb.Length > 0)
				{
					token = new Token(TokenType.String, sb.ToString(), null, currentLine);
					tokens.Add(token);
				}

				token = new Token(TokenType.InterpolatedStringEnd, "\"", null, currentLine);
				tokens.Add(token);

				return new ScanTokenResult(current - startOfString + 2, newLines, tokens);
			}

			sb.Append(c);
		}

		Morph.Error(currentLine, "Unterminated string.");
		return new ScanTokenResult(current - startOfString, newLines);
	}

    private static ScanTokenResult ScanNumber(string source, int current, int currentLine)
    {
        Debug.Assert(char.IsDigit(source[current]));

		int startIndex = current;

		while (!IsAtEnd(source, current) && char.IsDigit(source[current]))
		{
			current++;
		}

		// Fractional numbers are followed by a '.' then another number
		if (IsAtEnd(source, current)
			|| source[current] != '.'
			|| IsAtEnd(source, current + 1)
			|| !char.IsDigit(source[current + 1]))
		{
			var nonFractionalToken = new Token(
				TokenType.Number,
				source[startIndex..current],
				decimal.Parse(source[startIndex..current]),
				currentLine);

			return new ScanTokenResult(current - startIndex, 0, nonFractionalToken);
		}

		current++; // Move past the '.'

		while (!IsAtEnd(source, current) && char.IsDigit(source[current]))
		{
			current++;
		}

		var fractionalToken = new Token(
			TokenType.Number,
			source[startIndex..current],
			decimal.Parse(source[startIndex..current]),
			currentLine);

		return new ScanTokenResult(current - startIndex, 0, fractionalToken);
	}

    private static ScanTokenResult ScanIdentifier(string source, int current, int currentLine)
    {
		Debug.Assert(char.IsLetter(source[current]));

		Token token;
		int startIndex = current;

		while (!IsAtEnd(source, current) && IsIdentifierChar(source[current]))
		{
			current++;
		}

        string text = source[startIndex..current];

        if (Lexemes.Keywords.TryGetValue(text, out TokenType type))
        {
			token = new Token(type, text, null, currentLine);
			return new ScanTokenResult(text.Length, 0, token);
        }

		token = new Token(TokenType.Identifier, text, null, currentLine);
		return new ScanTokenResult(text.Length, 0, token);
    }

	private static char Peek(string source, int current)
	{
		if (IsAtEnd(source, current))
		{
			return '\0';
		}

		return source[current + 1];
	}

	private static bool IsAtEnd(string source, int current)
	{
		return current >= source.Length;
	}

	private static bool IsIdentifierChar(char c)
	{
		return char.IsLetterOrDigit(c) || c == '_';
	}
}