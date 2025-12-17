namespace Morph.Scanning;

/// <summary>
/// Represents an individual unit of source code that has been produced
/// by the scanner
/// </summary>
internal class Token
{
	public static readonly Token Empty = new Token(TokenType.Eof, "", null, -1);

	/// <summary>
	/// The type of the token
	/// </summary>
	public TokenType Type { get; private set; }

	/// <summary>
	/// The sequence of characters in the source code that this token is derived from
	/// </summary>
    public string Lexeme { get; private set; }

	/// <summary>
	/// The parsed value of this token, if relevant
	/// </summary>
    public object? Literal { get; private set; }

	/// <summary>
	/// The line in the source code that contains this token
	/// </summary>
    public int Line { get; private set; }


    public Token(TokenType type, string lexeme, object? literal, int line)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override string ToString()
    {
        return $"{Type} {Lexeme} {Literal}";
    }
}