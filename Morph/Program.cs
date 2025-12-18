using Morph;

var inputs = new Dictionary<string, string>
{
};

var code = """
	var a = 5;

	var tickString = `
		hello world
		  
		[a]
	`;

	Output.Write(tickString);

	""";


MorphRunner.Out += (sender, message) =>
{
	Console.WriteLine($"[{message.channel}] {message.content}");
};

MorphRunner.RunCode(code, inputs);