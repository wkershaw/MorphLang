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

        public MorphInstance Call(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
        {
            return _body(interpreter, instance, arguments);
		}

        public override string ToString()
        {
            return "<nativeConstructor>";
        }
    }
}
