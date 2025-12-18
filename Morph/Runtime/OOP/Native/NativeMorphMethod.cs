using Morph.Runtime.Functions.Interfaces;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP.Native
{
	internal class NativeMorphMethod : IMorphMethod
    {
        private readonly NativeMorphFunctionBody _body;
		private readonly Environment _closure;

		public int Arity { get; private init; }

		public NativeMorphMethod(int arity, NativeMorphFunctionBody body, Environment closure)
		{
			Arity = arity;
            _body = body;
			_closure = closure;
		}

        public IMorphFunction Bind(MorphInstance instance)
        {
			var environment = new Environment(_closure);
			environment.Define("this", instance);

			return new NativeMorphFunction(Arity, _body, environment);
        }

        public override string ToString()
        {
			return "<nativeMorphMethod>";
        }
    }
}
