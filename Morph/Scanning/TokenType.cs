namespace Morph.Scanning;

/// <summary>
/// Represents the types of token that can be parsed by the scanner
/// </summary>
internal enum TokenType
{
    // Single-character tokens.
    LeftParen, RightParen, LeftBrace, RightBrace,
    LeftSquareBracket, RightSquareBracket,
    Comma, Dot, Minus, Plus, Semicolon, Slash, Star,

    // One or two character tokens.
    Bang, BangEqual,
    Equal, EqualEqual,
    Greater, GreaterEqual,
    Less, LessEqual,

    // Literals.
    Identifier, String, Number,
    InterpolatedStringStart, InterpolatedStringEnd, InterpolatedStringExpressionStart, InterpolatedStringExpressionEnd, 

    // Keywords.
    And, Else, False, For,
    Fun, If, Nil, Or,
    Return, True,
    Var, While,

    // OOP
    Class, This,

    // In Declaration
    In,

    Eof
}