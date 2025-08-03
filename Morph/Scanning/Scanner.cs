using System.Text;

namespace Morph.Scanning;

internal class Scanner
{
    private Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
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
        { "JSON", TokenType.Json },
        { "URL", TokenType.Url }
    };


    private string _source;
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private List<Token> _tokens = new List<Token>();

    public Scanner(string source)
    {
        _source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case '[':
                AddToken(TokenType.LeftSquareBracket);
                break;
            case ']':
                AddToken(TokenType.RightSquareBracket);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;

            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (!IsAtEnd() && _source[_current] != '\n')
                    {
                        Advance();
                    }
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;

            case '"':
                ScanString();
                break;

            case '$':
                if (Match('"'))
                {
                    ScanInterpolatedString();
                }
                else
                {
                    Morph.Error(_line, "Unexpected character after '$'");
                }
                break;

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;

            default:
                if (char.IsDigit(c))
                {
                    ScanNumber();
                }
                else if (char.IsLetter(c))
                {
                    ScanIdentifier();
                }
                else
                {
                    Morph.Error(_line, $"Unexpected character: {c}");
                }
                break;
        }
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || _source[_current] != expected) return false;
        _current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
		_start = _current;
    }

    private void ScanString()
    {
        var sb = new StringBuilder();
        bool escaped = false;

        while (!IsAtEnd())
        {
            char c = Peek();

            if (escaped)
            {
                if (c == '"' || c == '\\')
                {
                    sb.Append(c);
                }
                else if (c == 'n')
                {
                    sb.Append('\n');
                }
                else if (c == 'r')
                {
                    sb.Append('\r');
                }
                else if (c == 't')
                {
                    sb.Append('\t');
                }
                else
                {
                    Morph.Error(_line, $"Unable to escape character '{c}'");
                }
                escaped = false;
                Advance();
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                Advance();
                continue;
            }

            if (c == '"')
            {
                break;
            }

            if (c == '\n')
            {
                _line++;
            }
                
            sb.Append(c);
            Advance();
        }

        if (IsAtEnd())
        {
            Morph.Error(_line, "Unterminated string.");
            return;
        }

        Advance(); // Consume closing quote

        AddToken(TokenType.String, sb.ToString());
    }

    private void ScanInterpolatedString()
    {
        // Emit start token for interpolated string
        AddToken(TokenType.InterpolatedStringStart);

        var sb = new StringBuilder();
        bool escaped = false;

        while (!IsAtEnd())
        {
            char c = Peek();

            if (escaped)
            {
                switch (c)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    default:
                        Morph.Error(_line, $"Unable to escape character '{c}'");
                        break;
                }
                escaped = false;
                Advance();
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                Advance();
                continue;
            }

            if (c == '[')
            {
                // Emit any string part before the expression
                if (sb.Length > 0)
                {
                    AddToken(TokenType.String, sb.ToString());
                    sb.Clear();
                }

                Advance(); // Consume '['
                AddToken(TokenType.InterpolatedStringExpressionStart);

				while (Peek() != ']' && !IsAtEnd())
				{
					ScanToken();
				}

                if (IsAtEnd())
                {
                    Morph.Error(_line, "Unterminated interpolation in string.");
                    break;
                }

                // The expression source is between exprStart and _current
                // The parser will handle the expression tokens after InterpolatedStringExpressionStart
                Advance(); // Consume ']'
                AddToken(TokenType.InterpolatedStringExpressionEnd);
                continue;
            }

            if (c == '"')
            {
                break;
            }

            if (c == '\n') _line++;
            sb.Append(c);
            Advance();
        }

        if (IsAtEnd())
        {
            Morph.Error(_line, "Unterminated interpolated string.");
            return;
        }

        Advance(); // Consume closing quote

        // Emit any trailing string part
        if (sb.Length > 0)
        {
            AddToken(TokenType.String, sb.ToString());
        }

        AddToken(TokenType.InterpolatedStringEnd);
    }

    private void ScanNumber()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }

        // Look for a fractional part.
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            // Consume the '.'.
            Advance();

            while (char.IsDigit(Peek()))
            {
                Advance();
            }
        }

        string number = _source.Substring(_start, _current - _start);
        AddToken(TokenType.Number, decimal.Parse(number));
    }

    private void ScanIdentifier()
    {
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            Advance();
        }

        string text = _source.Substring(_start, _current - _start);

        if (keywords.TryGetValue(text, out TokenType type))
        {
            AddToken(type);
        }
        else
        {
            AddToken(TokenType.Identifier);
        }
    }
}