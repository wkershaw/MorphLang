namespace Morph.Runtime.Functions.Interfaces
{
    internal interface IMorphFunction
    {
		int Arity { get; }
        object? Call(Interpreter interpreter, List<object?> arguments);
        string ToString();
    }
}