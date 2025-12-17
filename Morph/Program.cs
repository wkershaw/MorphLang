using Morph;

var inputs = new Dictionary<string, string>
{
};

var code = """
	// A test morph file

	fun HelloWorld()
	{
		WriteLine("Hello from Morph!");
	}

	HelloWorld();

	""";


MorphRunner.Out += (sender, message) =>
{
	Console.WriteLine($"[{message.channel}] {message.content}");
};

MorphRunner.RunCode(code, inputs);