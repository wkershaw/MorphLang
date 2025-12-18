using Morph.Runtime.Functions.Interfaces;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP.Native
{
	internal class NativeMorphMethod : IMorphMethod
    {
        private readonly NativeMorphFunctionBody _body;

		public int Arity { get; private init; }

		public NativeMorphMethod(int arity, NativeMorphFunctionBody body)
		{
			Arity = arity;
            _body = body;
        }

        public IMorphFunction Bind(MorphInstance instance)
        {
			return new NativeMorphFunction(Arity, _body);
        }

        public override string ToString()
        {
			return "<nativeMorphMethod>";
        }
    }
}
