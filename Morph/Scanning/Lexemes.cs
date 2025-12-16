using System.Collections.Frozen;

namespace Morph.Scanning
{
    internal static class Lexemes
    {
		public static FrozenDictionary<string, TokenType> Keywords;

		public static FrozenDictionary<char, TokenType> SingleCharacterTokens;

		public static FrozenDictionary<char, TokenType> DoubleCharacterTokenStarts;

		public static FrozenDictionary<string, TokenType> DoubleCharacterTokens;

		static Lexemes()
		{
			Keywords = new Dictionary<string, TokenType>()
			{
				{ "and", TokenType.And },
				{ "else", TokenType.Else },
				{ "false", TokenType.False },
				{ "for", TokenType.For },
				{ "fun", TokenType.Fun },
				{ "if", TokenType.If },
				{ "nil", TokenType.Nil },
				{ "or", TokenType.Or },
				{ "return", TokenType.Return },
				{ "true", TokenType.True },
				{ "var", TokenType.Var },
				{ "while", TokenType.While },
				{ "in", TokenType.In },
				{ "class", TokenType.Class },
				{ "this", TokenType.This }
			}
			.ToFrozenDictionary();

			SingleCharacterTokens = new Dictionary<char, TokenType>()
			{
				{ '(', TokenType.LeftParen },
				{ ')', TokenType.RightParen },
				{ '{', TokenType.LeftBrace },
				{ '}', TokenType.RightBrace },
				{ '[', TokenType.LeftSquareBracket },
				{ ']', TokenType.RightSquareBracket },
				{ ',', TokenType.Comma },
				{ '.', TokenType.Dot },
				{ '-', TokenType.Minus },
				{ '+', TokenType.Plus },
				{ ';', TokenType.Semicolon },
				{ '*', TokenType.Star },
			}
			.ToFrozenDictionary();

			DoubleCharacterTokenStarts = new Dictionary<char, TokenType>()
			{
				{ '!', TokenType.Bang},
				{ '=', TokenType.Equal},
				{ '>', TokenType.Greater},
				{ '<', TokenType.Less },
			}
			.ToFrozenDictionary();

			DoubleCharacterTokens = new Dictionary<string, TokenType>()
			{
				{ "!=", TokenType.BangEqual },
				{ "==", TokenType.EqualEqual },
				{ "<=", TokenType.LessEqual },
				{ ">=", TokenType.GreaterEqual },
			}
			.ToFrozenDictionary();
		}
	}
}
