using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Random : MorphClass
{
	public Random(Environment closure) : base("Random")
	{
		var constructor = new NativeMorphConstructor(0, Constructor, closure);
		AddConstructor(constructor);

		var between = new NativeMorphMethod(2, Between, closure);
		AddMethod("Between", between);
	}

	public static MorphInstance CreateStaticInstance(Environment closure)
	{
		return new MorphInstance(new Random(closure));
	}

	private MorphInstance Constructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
	{
		throw new RuntimeException(null, "Cannot create new instance of Random class");
	}

	private object? Between(Interpreter interpreter, List<object?> arguments, Environment closure)
	{
		if (!int.TryParse(arguments[0]?.ToString(), out int lowerBound))
		{
			throw new RuntimeException(null, "Argument must be an int");
		}

		if (!int.TryParse(arguments[1]?.ToString(), out int upperBound))
		{
			throw new RuntimeException(null, "Argument must be an int");
		}

		var random = new System.Random();
		return random.Next(lowerBound, upperBound);
	}
}