namespace Morph.Runtime.NativeFunctions;

internal class SetHeaderCallable : IMorphCallable
{
    public int Arity => 2;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        string key = interpreter.Stringify(arguments[0]);
        if (!key.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
        {
            throw new RuntimeException(null, "Header key contains invalid characters.");
        }

        string value = interpreter.Stringify(arguments[1]);
        if (!value.All(c => char.IsLetterOrDigit(c)))
        {
            throw new RuntimeException(null, "Header value contains invalid characters.");
        }

        Morph.Output("header", $"{key}:{value}");
        return null;
    }
}