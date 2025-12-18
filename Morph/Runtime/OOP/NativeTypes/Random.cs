using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Random : MorphClass
{
	public static MorphInstance Instance = new MorphInstance(new Random());

	public Random() : base("Random")
	{
		var constructor = new NativeMorphConstructor(0, (_, _, _) => throw new RuntimeException(null, "Cannot create new instance of Output class"));
		AddConstructor(constructor);

		var between = new NativeMorphMethod(2, Between);
		AddMethod("Between", between);
	}

	private object? Between(Interpreter interpreter, List<object?> arguments)
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