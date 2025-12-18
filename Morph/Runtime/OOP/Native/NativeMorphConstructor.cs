using Morph.Runtime.Functions;
using Morph.Runtime.Functions.Interfaces;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP.Native
{
    internal class NativeMorphConstructor : IMorphConstructor
    {
		private readonly Func<Interpreter, MorphInstance, List<object?>, MorphInstance> _body;

        public int Arity { get; private init; }

		public NativeMorphConstructor(int arity, Func<Interpreter, MorphInstance, List<object?>, MorphInstance> body)
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
