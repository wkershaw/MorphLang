using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Clock : MorphClass
{
	public static MorphInstance Instance = new MorphInstance(new Clock());

	public Clock() : base("Clock")
	{
		var constructor = new NativeMorphConstructor(0, (_, _, _) => throw new RuntimeException(null, "Cannot create new instance of Output class"));
		AddConstructor(constructor);

		var toFormattedString = new NativeMorphMethod(1, ToFormattedString);
		AddMethod("ToFormattedString", toFormattedString);
	}

	private string ToFormattedString(Interpreter interpreter, List<object?> arguments)
	{
		string format = interpreter.Stringify(arguments[0]);
		return DateTime.Now.ToString(format);
	}
}