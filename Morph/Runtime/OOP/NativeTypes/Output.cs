using Morph.Runtime.OOP.Native;
namespace Morph.Runtime.OOP.NativeTypes
{
    internal class Output : MorphClass
    {
		public static MorphInstance Instance = new MorphInstance(new Output());

        public Output() : base("Output")
        {
			var constructor = new NativeMorphConstructor(0, (_, _, _) => throw new RuntimeException(null, "Cannot create new instance of Output class"));
			AddConstructor(constructor);

			var debug = new NativeMorphMethod(1, Debug);
			AddMethod("Debug", debug);

			var error = new NativeMorphMethod(1, Error);
			AddMethod("Error", error);

			var setHeader = new NativeMorphMethod(2, SetHeader);
			AddMethod("SetHeader", setHeader);

			var setResponseCode = new NativeMorphMethod(1, SetResponseCode);
			AddMethod("SetResponseCode", setResponseCode);

			var write = new NativeMorphMethod(1, Write);
			AddMethod("Write", write);

			var writeLine = new NativeMorphMethod(1, WriteLine);
			AddMethod("WriteLine", writeLine);
		}

		private object? Debug(Interpreter interpreter, List<object?> arguments)
		{
			MorphRunner.Output("debug", interpreter.Stringify(arguments[0]));

			return null;
		}

		private object? Error(Interpreter interpreter, List<object?> arguments)
		{
			MorphRunner.Output("error", interpreter.Stringify(arguments[0]));

			return null;
		}

		private object? SetHeader(Interpreter interpreter, List<object?> arguments)
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

		private object? SetResponseCode(Interpreter interpreter, List<object?> arguments)
		{
			if (!int.TryParse(arguments[0]?.ToString(), out int statusCode))
			{
				throw new RuntimeException(null, "Status code must be an integer.");
			}

			MorphRunner.Output("response", statusCode.ToString());
			return null;
		}

		public object? Write(Interpreter interpreter, List<object?> arguments)
		{
			string value = interpreter.Stringify(arguments[0]);
			MorphRunner.Output("write", value);
			return null;
		}

		public object? WriteLine(Interpreter interpreter, List<object?> arguments)
		{
			string value = interpreter.Stringify(arguments[0]) + "\n";
			MorphRunner.Output("write", value);
			return null;
		}
	}
}
