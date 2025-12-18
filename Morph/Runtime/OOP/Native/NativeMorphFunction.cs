using Morph.Runtime.Functions.Interfaces;

namespace Morph.Runtime.OOP.Native
{
    internal class NativeMorphFunction : IMorphFunction
    {
		private readonly Func<Interpreter, List<object?>, object?> _body;

		public int Arity { get; private init; }

		public NativeMorphFunction(int arity, Func<Interpreter, List<object?>, object?> body)
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
