namespace Morph.Runtime.NativeFunctions;

internal class WriteCallable : IMorphCallable
{
    public int Arity => 1;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        string value = interpreter.Stringify(arguments[0]);
        Morph.Output("write", value);
        return null;
    }
}