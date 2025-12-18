using Morph;

var inputs = new Dictionary<string, string>
{
};

var code = """
	class MyClass
	{
		init()
		{
			this.x = "hello world";
		}

		say()
		{
			Output.WriteLine(this.x);
		}
	}

	var i = MyClass();
	i.say();

	""";


MorphRunner.Out += (sender, message) =>
{
	Console.WriteLine($"[{message.channel}] {message.content}");
};

MorphRunner.RunCode(code, inputs);