namespace Morph.Runtime.NativeFunctions;

internal class SetResponseCodeCallable : IMorphCallable
{
    public int Arity => 1;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        if (!int.TryParse(arguments[0]?.ToString(), out int statusCode))
        {
            throw new RuntimeException(null, "Status code must be an integer.");
        }

        MorphRunner.Output("response", statusCode.ToString());
        return null;
    }
}