namespace Morph.Runtime.NativeFunctions;

internal class DebugCallable : IMorphCallable
{
    public int Arity => 1;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        string value = interpreter.Stringify(arguments[0]);
        Morph.Output("debug", value);
        return null;
    }
}