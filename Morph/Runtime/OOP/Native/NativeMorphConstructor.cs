using Morph.Runtime.Functions.Interfaces;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP.Native
{
	internal delegate MorphInstance NativeMorphConstructorBody(Interpreter interpreter, MorphInstance instance, List<object?> arguments);

	internal class NativeMorphConstructor : IMorphConstructor
    {
		private readonly NativeMorphConstructorBody _body;

        public int Arity { get; private init; }

		public NativeMorphConstructor(int arity, NativeMorphConstructorBody body)
        {
			Arity = arity;
            _body = body;
        }

		public IMorphFunction Bind(MorphInstance instance)
		{
			return new NativeMorphFunction(Arity, (i, a) => _body(i, instance, a));
		}

		public override string ToString()
        {
            return "<nativeConstructor>";
        }
    }
}
