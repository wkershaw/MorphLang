using Morph.Runtime.Functions.Interfaces;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP.Native
{
	internal delegate MorphInstance NativeMorphConstructorBody(Interpreter interpreter, MorphInstance instance, List<object?> arguments);

	internal class NativeMorphConstructor : IMorphConstructor
    {
		private readonly NativeMorphConstructorBody _body;
		private readonly Environment _closure;

		public int Arity { get; private init; }

		public NativeMorphConstructor(int arity, NativeMorphConstructorBody body, Environment closure)
        {
			Arity = arity;
            _body = body;
			_closure = closure;
		}

		public IMorphFunction Bind(MorphInstance instance)
		{
			var environment = new Environment(_closure);
			environment.Define("this", instance);

			NativeMorphFunctionBody functionBody = (i,a,e) => _body(i, instance, a);

			return new NativeMorphFunction(Arity, functionBody, environment);
		}

		public override string ToString()
        {
            return "<nativeConstructor>";
        }
    }
}
