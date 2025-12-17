namespace Morph.Runtime.NativeFunctions;

internal class DebugCallable : IMorphCallable
{
    public int Arity => 1;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        string value = interpreter.Stringify(arguments[0]);
        MorphRunner.Output("debug", value);
        return null;
    }
}