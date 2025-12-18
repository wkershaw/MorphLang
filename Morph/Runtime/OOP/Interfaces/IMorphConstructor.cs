using Morph.Runtime.Functions.Interfaces;

namespace Morph.Runtime.OOP.Interfaces
{
    internal interface IMorphConstructor
    {
        int Arity { get; }

		IMorphFunction Bind(MorphInstance instance);

		string ToString();
    }
}