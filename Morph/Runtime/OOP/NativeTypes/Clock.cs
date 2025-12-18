using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;
internal class Clock : MorphClass
{
	public Clock(Environment closure) : base("Clock")
	{
		var constructor = new NativeMorphConstructor(0, Constructor, closure);
		AddConstructor(constructor);

		var toFormattedString = new NativeMorphMethod(1, ToFormattedString, closure);
		AddMethod("ToFormattedString", toFormattedString);

		var toString = new NativeMorphMethod(0, ToString, closure);
		AddMethod("ToString", toString);
	}

	public static MorphInstance CreateStaticInstance(Environment closure)
	{
		return new MorphInstance(new Clock(closure));
	}

	private MorphInstance Constructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
	{
		throw new RuntimeException(null, "Cannot create new instance of Clock class");
	}

	private string ToFormattedString(Interpreter interpreter, List<object?> arguments, Environment environment)
	{
		string format = interpreter.Stringify(arguments[0]);
		return DateTime.Now.ToString(format);
	}

	private string ToString(Interpreter interpreter, List<object?> arguments, Environment environment)
	{
		return DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
	}
}