namespace Morph.Runtime.NativeFunctions;

internal class WriteLineCallable : IMorphCallable
{
    public int Arity => 1;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        string value = interpreter.Stringify(arguments[0]) + "\n";
        MorphRunner.Output("write", value);
        return null;
    }
}