namespace Morph.Runtime;

internal interface IMorphCallable
{
    int Arity { get; }

    object? Call(Interpreter interpreter, List<object?> arguments);
}

internal interface IMorphInstanceCallable : IMorphCallable
{
	IMorphCallable Bind(MorphInstance instance);
}