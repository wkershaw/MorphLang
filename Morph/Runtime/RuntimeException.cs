using Morph.Scanning;

internal class RuntimeException : Exception
{
    public Token? Token { get; private set; }

    public RuntimeException(Token? token, string message) : base(message)
    {
        Token = token;
    }
}