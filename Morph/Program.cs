using Morph;

var inputs = new Dictionary<string, string>
{
};

var code = """
	Output.WriteLine(Clock.ToFormattedString("dd-mm-yy"));
	Output.WriteLine(Random.Between(4,40));

	var url = Url("http://www.google.com?a=b");
	Output.WriteLine(url["a"]);

	var json = Json("{ \"test\": \"value\" }");
	Output.WriteLine(json["test"]);

	""";


MorphRunner.Out += (sender, message) =>
{
	Console.WriteLine($"[{message.channel}] {message.content}");
};

MorphRunner.RunCode(code, inputs);