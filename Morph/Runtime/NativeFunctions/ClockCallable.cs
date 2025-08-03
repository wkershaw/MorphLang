namespace Morph.Runtime.NativeFunctions;

internal class ClockCallable : IMorphCallable
{
    public int Arity => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        return (decimal)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}