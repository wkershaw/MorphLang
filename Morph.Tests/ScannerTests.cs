using Morph.Scanning;

namespace Morph.Tests;

public class ScannerTests
{
    [Fact]
    public void EmptySourceResultsInEofToken()
    {
        var source = "";

		var tokens = ScanSource(source);

		Assert.Single(tokens, t => t.Type == TokenType.Eof);
    }

	[Fact]
	public void AllTokenTypesCanBeScanned()
	{
		var source = """
			( ) { } [ ] , . - + ; / *
			! != = == > >= < <=
			abc "abc" 123 $"[world]"
			and else false for fun
			if nil or return true var while
			class this
			in
			""";

		var tokens = ScanSource(source);

		Console.WriteLine(string.Join(Environment.NewLine, tokens.Select(t => $"{t.Type}: '{t.Lexeme}'")));

		Assert.Collection(tokens,
			t => Assert.Equal(TokenType.LeftParen, t.Type),
			t => Assert.Equal(TokenType.RightParen, t.Type),
			t => Assert.Equal(TokenType.LeftBrace, t.Type),
			t => Assert.Equal(TokenType.RightBrace, t.Type),
			t => Assert.Equal(TokenType.LeftSquareBracket, t.Type),
			t => Assert.Equal(TokenType.RightSquareBracket, t.Type),
			t => Assert.Equal(TokenType.Comma, t.Type),
			t => Assert.Equal(TokenType.Dot, t.Type),
			t => Assert.Equal(TokenType.Minus, t.Type),
			t => Assert.Equal(TokenType.Plus, t.Type),
			t => Assert.Equal(TokenType.Semicolon, t.Type),
			t => Assert.Equal(TokenType.Slash, t.Type),
			t => Assert.Equal(TokenType.Star, t.Type),
			t => Assert.Equal(TokenType.Bang, t.Type),
			t => Assert.Equal(TokenType.BangEqual, t.Type),
			t => Assert.Equal(TokenType.Equal, t.Type),
			t => Assert.Equal(TokenType.EqualEqual, t.Type),
			t => Assert.Equal(TokenType.Greater, t.Type),
			t => Assert.Equal(TokenType.GreaterEqual, t.Type),
			t => Assert.Equal(TokenType.Less, t.Type),
			t => Assert.Equal(TokenType.LessEqual, t.Type),
			t => Assert.Equal(TokenType.Identifier, t.Type),
			t => Assert.Equal(TokenType.String, t.Type),
			t => Assert.Equal(TokenType.Number, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringStart, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringExpressionStart, t.Type),
			t => Assert.Equal(TokenType.Identifier, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringExpressionEnd, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringEnd, t.Type),
			t => Assert.Equal(TokenType.And, t.Type),
			t => Assert.Equal(TokenType.Else, t.Type),
			t => Assert.Equal(TokenType.False, t.Type),
			t => Assert.Equal(TokenType.For, t.Type),
			t => Assert.Equal(TokenType.Fun, t.Type),
			t => Assert.Equal(TokenType.If, t.Type),
			t => Assert.Equal(TokenType.Nil, t.Type),
			t => Assert.Equal(TokenType.Or, t.Type),
			t => Assert.Equal(TokenType.Return, t.Type),
			t => Assert.Equal(TokenType.True, t.Type),
			t => Assert.Equal(TokenType.Var, t.Type),
			t => Assert.Equal(TokenType.While, t.Type),
			t => Assert.Equal(TokenType.Class, t.Type),
			t => Assert.Equal(TokenType.This, t.Type),
			t => Assert.Equal(TokenType.In, t.Type),
			t => Assert.Equal(TokenType.Eof, t.Type));
	}

	[Fact]
	public void InterpolatedStringIsCorrectlyScanned()
	{
		var source = """
			$"This is an [1 + 2] interpolated \" \\ \n \r \t \[ \] string "
			""";
		
		var tokens = ScanSource(source);

		Assert.Collection(tokens,
			t => Assert.Equal(TokenType.InterpolatedStringStart, t.Type),
			t => Assert.Equal(TokenType.String, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringExpressionStart, t.Type),
			t => Assert.Equal(TokenType.Number, t.Type),
			t => Assert.Equal(TokenType.Plus, t.Type),
			t => Assert.Equal(TokenType.Number, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringExpressionEnd, t.Type),
			t => Assert.Equal(TokenType.String, t.Type),
			t => Assert.Equal(TokenType.InterpolatedStringEnd, t.Type),
			t => Assert.Equal(TokenType.Eof, t.Type));
	}

	[Fact]
	public void CharactersAreCorrectlyEscapedInStrings()
	{
		var source = """
			"This is a string [ ] \" \\ \n \r \t \[ \] string "
			""";

		var tokens = ScanSource(source);

		Assert.Collection(tokens,
			t => Assert.Equal(TokenType.String, t.Type),
			t => Assert.Equal(TokenType.Eof, t.Type));
	}

	private List<Token> ScanSource(string source)
	{
		return Scanner.ScanTokens(source);
	}
}