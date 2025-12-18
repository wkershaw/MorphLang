using Morph.Runtime.OOP.Native;
namespace Morph.Runtime.OOP.NativeTypes
{
	internal class Output : MorphClass
	{
		public Output(Environment closure) : base("Output")
		{
			var constructor = new NativeMorphConstructor(0, Constructor, closure);
			AddConstructor(constructor);

			var debug = new NativeMorphMethod(1, Debug, closure);
			AddMethod("Debug", debug);

			var error = new NativeMorphMethod(1, Error, closure);
			AddMethod("Error", error);

			var setHeader = new NativeMorphMethod(2, SetHeader, closure);
			AddMethod("SetHeader", setHeader);

			var setResponseCode = new NativeMorphMethod(1, SetResponseCode, closure);
			AddMethod("SetResponseCode", setResponseCode);

			var write = new NativeMorphMethod(1, Write, closure);
			AddMethod("Write", write);

			var writeLine = new NativeMorphMethod(1, WriteLine, closure);
			AddMethod("WriteLine", writeLine);
		}

		public static MorphInstance CreateStaticInstance(Environment closure)
		{
			return new MorphInstance(new Output(closure));
		}

		private MorphInstance Constructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
		{
			throw new RuntimeException(null, "Cannot create new instance of Output class");
		}

		private object? Debug(Interpreter interpreter, List<object?> arguments, Environment closure)
		{
			MorphRunner.Output("debug", interpreter.Stringify(arguments[0]));

			return null;
		}

		private object? Error(Interpreter interpreter, List<object?> arguments, Environment closure)
		{
			MorphRunner.Output("error", interpreter.Stringify(arguments[0]));

			return null;
		}

		private object? SetHeader(Interpreter interpreter, List<object?> arguments, Environment closure)
		{
			string key = interpreter.Stringify(arguments[0]);
			if (!key.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
			{
				throw new RuntimeException(null, "Header key contains invalid characters.");
			}

			string value = interpreter.Stringify(arguments[1]);
			if (!value.All(c => char.IsLetterOrDigit(c)))
			{
				throw new RuntimeException(null, "Header value contains invalid characters.");
			}

			MorphRunner.Output("header", $"{key}:{value}");
			return null;
		}

		private object? SetResponseCode(Interpreter interpreter, List<object?> arguments, Environment closure)
		{
			if (!int.TryParse(arguments[0]?.ToString(), out int statusCode))
			{
				throw new RuntimeException(null, "Status code must be an integer.");
			}

			MorphRunner.Output("response", statusCode.ToString());
			return null;
		}

		public object? Write(Interpreter interpreter, List<object?> arguments, Environment closure)
		{
			string value = interpreter.Stringify(arguments[0]);
			MorphRunner.Output("write", value);
			return null;
		}

		public object? WriteLine(Interpreter interpreter, List<object?> arguments, Environment closure)
		{
			string value = interpreter.Stringify(arguments[0]) + "\n";
			MorphRunner.Output("write", value);
			return null;
		}
	}
}
