using Morph.Runtime.Functions.Interfaces;

namespace Morph.Runtime.OOP.Interfaces
{
    internal interface IMorphMethod
    {
		public int Arity { get; }

		IMorphFunction Bind(MorphInstance instance);
        
		string ToString();
    }
}