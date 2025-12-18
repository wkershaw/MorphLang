using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    /// <param name="NewLines">The number of new line characters encountered during the scan.</param>
    /// <param name="Tokens">A collection of tokens identified during the scan. May contain zero or more tokens.</param>
    private record ScanTokenResult(int NewLines, params IEnumerable<Token> Tokens);

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

        List<Token> tokens = [];

        int currentLine = 1;
		var helper = new ListHelper<char>(source.ToCharArray(), '\0');

		while (!helper.IsAtEnd())
		{
			ScanTokenResult scanResult = ScanNextTokens(helper, currentLine);
			currentLine += scanResult.NewLines;
			tokens.AddRange(scanResult.Tokens);
		}

        return tokens;
    }

    /// <summary>
    /// Scans the next token or sequence of tokens from the source code, starting at the specified position and line
    /// number.
    /// </summary>
    private static ScanTokenResult ScanNextTokens(ListHelper<char> source, int currentLine)
    {
		Token token;
		
		source.MoveNext();
        char c = source.Current;
        int start = source.CurrentIndex;

        // Single character tokens
        if (Lexemes.SingleCharacterTokens.TryGetValue(c, out TokenType singleCharToken))
        {
            token = new Token(singleCharToken, c.ToString(), null, currentLine);
            return new ScanTokenResult(0, token);
        }

        // Single or double character tokens
		if (CheckDoubleCharacterToken(source, currentLine, out ScanTokenResult? doubleCharTokenResult))
		{
			return doubleCharTokenResult;
		}

        if (c == '/')
        {
            if (source.Peek() != '/')
            {
                return new ScanTokenResult(0, new Token(TokenType.Slash, "/", null, currentLine));
            }

            // Comment. So scan to the end of the line or file
            while (!source.IsAtEnd() && source.Current != '\n')
            {
                source.MoveNext();
            }

            return new ScanTokenResult(0);
        }

        if (c == '"')
        {
            return ScanString(source, currentLine);
        }

		if (c == '`')
		{
			return ScanTickString(source, currentLine);
		}

        if (c == '$')
        {
            if (source.Peek() == '"')
            {
                return ScanInterpolatedString(source, currentLine);
            }

            MorphRunner.Error(currentLine, "Unexpected character after '$'");
            return new ScanTokenResult(0);
        }

        if (c == '\n')
        {
            return new ScanTokenResult(1);
        }

        if (char.IsWhiteSpace(c))
        {
            return new ScanTokenResult(0);
        }

        if (char.IsDigit(c))
        {
            return ScanNumber(source, currentLine);
        }

        if (char.IsLetter(c))
        {
            return ScanIdentifier(source, currentLine);
        }

		if (c == '\0')
		{
			return new ScanTokenResult(0, new Token(TokenType.Eof, "", null, currentLine));
		}

        // Reached an unexpected character, but fail gracefully and attempt
        // to scan the rest of the source to spot any more errors
        MorphRunner.Error(currentLine, $"Unexpected character: {c}");
        return new ScanTokenResult(0);
    }

	private static bool CheckDoubleCharacterToken(ListHelper<char> source, int line, [MaybeNullWhen(false)]out ScanTokenResult result)
	{
		char c = source.Current;

		if (!Lexemes.DoubleCharacterTokenStarts.TryGetValue(c, out TokenType tokenType))
		{
			result = null;
			return false;
		}
		
		// Could still be a double character token, so check the next char
		var nextChar = source.Peek();
		var possibleDoubleCharToken = $"{c}{nextChar}";

		if (Lexemes.DoubleCharacterTokens.TryGetValue(possibleDoubleCharToken, out TokenType doubleCharToken))
		{
			source.MoveNext(); // Consume the second character
			result = new ScanTokenResult(0, new Token(doubleCharToken, possibleDoubleCharToken, null, line));
			return true;
		}

		// Nope, just a single character token
		result = new ScanTokenResult(0, new Token(tokenType, c.ToString(), null, line));

		return true;
	}

	private static ScanTokenResult ScanString(ListHelper<char> source, int currentLine)
    {
        Debug.Assert(source.Current == '"');

        StringBuilder sb = new();
        bool escaped = false;
        int newLines = 0;

        while (source.MoveNext())
        {
            char c = source.Current;

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
                    MorphRunner.Error(currentLine, $"Unable to escape character '{c}'");
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
                Token token = new Token(TokenType.String, sb.ToString(), sb.ToString(), currentLine);
                return new ScanTokenResult(newLines, token);
            }

            sb.Append(c);
        }

        MorphRunner.Error(currentLine, "Unterminated string.");
        return new ScanTokenResult(newLines);
    }

	private static ScanTokenResult ScanTickString(ListHelper<char> source, int currentLine)
	{
		Debug.Assert(source.Current == '`');
		source.MoveNext();

		List<Token> tokens = [];
		StringBuilder sb = new();
		bool escaped = false;
		int newLines = 0;

		Token startToken = new(TokenType.InterpolatedStringStart, "`", null, currentLine);
		tokens.Add(startToken);

		while (source.MoveNext())
		{
			char c = source.Current;

			if (escaped)
			{
				escaped = false;

				var escapedCharacter = c switch
				{
					'`' => '`',
					'\\' => '\\',
					'n' => '\n',
					'r' => '\r',
					't' => '\t',
					'[' => '[',
					_ => '_'
				};

				if (escapedCharacter == '_')
				{
					MorphRunner.Error(currentLine, $"Unable to escape character '{c}'");
					continue;
				}

				_ = sb.Append(escapedCharacter);
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
				sb.Append(c);

				tokens.Add(new Token(TokenType.String, sb.ToString(), sb.ToString(), currentLine));
				sb.Clear();

				continue;
			}

			if (c == '[')
			{
				// Emit any string part before the expression
				if (sb.Length > 0)
				{
					tokens.Add(new Token(TokenType.String, sb.ToString(), sb.ToString(), currentLine));
					sb.Clear();
				}

				tokens.Add(new Token(TokenType.InterpolatedStringExpressionStart, sb.ToString(), null, currentLine));

				while (!source.IsAtEnd() && source.Peek() != ']')
				{
					ScanTokenResult expressionTokenResult = ScanNextTokens(source, currentLine);
					currentLine += expressionTokenResult.NewLines;
					tokens.AddRange(expressionTokenResult.Tokens);
				}

				if (source.IsAtEnd())
				{
					MorphRunner.Error(currentLine, "Unterminated interpolation in string.");
					break;
				}

				tokens.Add(new Token(TokenType.InterpolatedStringExpressionEnd, sb.ToString(), null, currentLine));

				// Consume the closing ']'
				source.MoveNext();

				continue;
			}

			if (c == '`')
			{
				var finalString = sb.ToString();

				if (!string.IsNullOrWhiteSpace(finalString) || tokens.Last().Type != TokenType.String)
				{
					MorphRunner.Error(currentLine, "Ending of tick string must be on its own line");
				}
				
				tokens.Insert(1, new Token(TokenType.TickStringPadding, finalString, finalString, currentLine));

				tokens.Add(new Token(TokenType.InterpolatedStringEnd, "`", null, currentLine));

				return new ScanTokenResult(newLines, tokens);
			}

			sb.Append(c);
		}

		MorphRunner.Error(currentLine, "Unterminated string.");
		return new ScanTokenResult(newLines, tokens);
	}

    private static ScanTokenResult ScanInterpolatedString(ListHelper<char> source, int currentLine)
    {
        Debug.Assert(source.Current == '$');
		source.MoveNext();
        Debug.Assert(source.Current == '"');

        List<Token> tokens = [];
        StringBuilder sb = new();
        bool escaped = false;
        int newLines = 0;

        Token startToken = new(TokenType.InterpolatedStringStart, "$\"", null, currentLine);
        tokens.Add(startToken);

		while (source.MoveNext())
        {
            char c = source.Current;

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
                    MorphRunner.Error(currentLine, $"Unable to escape character '{c}'");
                    continue;
                }

                _ = sb.Append(escapedCharacter);
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
                    tokens.Add(new Token(TokenType.String, sb.ToString(), sb.ToString(), currentLine));
                    sb.Clear();
                }

                tokens.Add(new Token(TokenType.InterpolatedStringExpressionStart, sb.ToString(), null, currentLine));

                while (!source.IsAtEnd() && source.Peek() != ']')
                {
                    ScanTokenResult expressionTokenResult = ScanNextTokens(source, currentLine);
                    currentLine += expressionTokenResult.NewLines;
                    tokens.AddRange(expressionTokenResult.Tokens);
                }

                if (source.IsAtEnd())
                {
                    MorphRunner.Error(currentLine, "Unterminated interpolation in string.");
                    break;
                }

                tokens.Add(new Token(TokenType.InterpolatedStringExpressionEnd, sb.ToString(), null, currentLine));

				// Consume the closing ']'
				source.MoveNext();

                continue;
            }

            if (c == '"')
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new Token(TokenType.String, sb.ToString(), sb.ToString(), currentLine));
                }

                tokens.Add(new Token(TokenType.InterpolatedStringEnd, "\"", null, currentLine));

                return new ScanTokenResult(newLines, tokens);
            }

            sb.Append(c);
        }

        MorphRunner.Error(currentLine, "Unterminated string.");
        return new ScanTokenResult(newLines, tokens);
    }

    private static ScanTokenResult ScanNumber(ListHelper<char> source, int currentLine)
    {
        Debug.Assert(char.IsDigit(source.Current));

		source.StartRange();

        while (!source.IsAtEnd() && char.IsDigit(source.Peek()))
        {
            source.MoveNext();
        }

        // Fractional numbers are followed by a '.' then another number
        if (source.Peek() != '.' || !char.IsDigit(source.PeekNext()))
        {
			// Not a fractional number, so just parse the integer part
			var integerLexeme = new string(source.GetRange());
			var integerValue = decimal.Parse(integerLexeme);

			Token integertoken = new(TokenType.Number, integerLexeme, integerValue, currentLine);
            return new ScanTokenResult(0, integertoken);
        }

		// Move over to the first digit of the decimal part
        source.MoveNext();
		source.MoveNext();

		while (!source.IsAtEnd() && char.IsDigit(source.Peek()))
        {
            source.MoveNext();
        }

		var fractionalLexeme = new string(source.GetRange());
		var fractionalValue = decimal.Parse(fractionalLexeme);

		Token nonFractionalToken = new(TokenType.Number, fractionalLexeme, fractionalValue, currentLine);
		return new ScanTokenResult(0, nonFractionalToken);
    }

    private static ScanTokenResult ScanIdentifier(ListHelper<char> source, int currentLine)
    {
        Debug.Assert(char.IsLetter(source.Current));

		source.StartRange();

        while (!source.IsAtEnd() && IsIdentifierChar(source.Peek()))
        {
            source.MoveNext();
        }

		string text = new string(source.GetRange());

        if (Lexemes.Keywords.TryGetValue(text, out TokenType type))
        {
            return new ScanTokenResult(0, new Token(type, text, null, currentLine));
        }

        return new ScanTokenResult(0, new Token(TokenType.Identifier, text, null, currentLine));
    }

    private static bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }
}