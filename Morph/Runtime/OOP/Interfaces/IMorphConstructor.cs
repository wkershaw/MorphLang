namespace Morph.Runtime.OOP.Interfaces
{
    internal interface IMorphConstructor
    {
        int Arity { get; }

        MorphInstance Call(Interpreter interpreter, MorphInstance instance, List<object?> arguments);
        string ToString();
    }
}