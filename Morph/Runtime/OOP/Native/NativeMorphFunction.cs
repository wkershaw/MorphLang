using Morph.Runtime.Functions.Interfaces;

namespace Morph.Runtime.OOP.Native
{
	internal delegate object? NativeMorphFunctionBody(Interpreter interpreter, List<object?> arguments, Environment environment);

	internal class NativeMorphFunction : IMorphFunction
    {
		private readonly NativeMorphFunctionBody _body;
		private readonly Environment _closure;

		public int Arity { get; private init; }

		public NativeMorphFunction(int arity, NativeMorphFunctionBody body, Environment closure)
		{
			Arity = arity;
			_body = body;
			_closure = closure;
		}

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
			Environment environment = new Environment(_closure);
			return _body(interpreter, arguments, environment);
        }

		public override string ToString()
		{
			return "<nativeMorphFunction>";
		}
	}
}
