namespace Morph.Parsing;

internal class ParseException : Exception
{
    public int Line { get; private set; }

    public ParseException(int line, string message) : base(message)
    {
        Line = line;
    }

    public override string ToString()
    {
        return $"[line {Line}] Error: {Message}";
    }
}