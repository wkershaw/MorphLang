using Morph.Runtime.Functions.Interfaces;

namespace Morph.Runtime.OOP.Native
{
	internal delegate object? NativeMorphFunctionBody(Interpreter interpreter, List<object?> arguments);


	internal class NativeMorphFunction : IMorphFunction
    {
		private readonly NativeMorphFunctionBody _body;

		public int Arity { get; private init; }

		public NativeMorphFunction(int arity, NativeMorphFunctionBody body)
		{
			_body = body;
			Arity = arity;
		}

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
			return _body(interpreter, arguments);
        }

		public override string ToString()
		{
			return "<nativeMorphFunction>";
		}
	}
}
