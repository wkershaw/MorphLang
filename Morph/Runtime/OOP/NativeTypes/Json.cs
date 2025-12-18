using Morph.Runtime.OOP.Native;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Json : MorphClass
{
	public Json(Environment closure) : base("Json")
	{
		var constructor = new NativeMorphConstructor(0, EmptyConstructor, closure);
		AddConstructor(constructor);

		constructor = new NativeMorphConstructor(1, StringConstructor, closure);
		AddConstructor(constructor);

		var index = new NativeMorphMethod(1, Index, closure);
		AddMethod("Index", index);

		var toString = new NativeMorphMethod(0, ToString, closure);
		AddMethod("ToString", toString);
	}

	private MorphInstance EmptyConstructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
	{
		var json = JsonSerializer.Deserialize<JsonNode>("");
		instance.Set("jsonString", json);
		return instance;
	}

	private MorphInstance StringConstructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
	{
		string jsonString = interpreter.Stringify(arguments[0]);
		try
		{
			var json = JsonSerializer.Deserialize<JsonNode>(jsonString);
			instance.Set("jsonString", json);
		}
		catch (JsonException ex)
		{
			throw new RuntimeException(null, $"Invalid JSON string: '{ex.Message}'");
		}

		return instance;
	}

	private MorphInstance JsonConstructor(MorphInstance instance, JsonNode? node)
	{
		instance.Set("jsonString", node);
		return instance;
	}

	private object? ToString(Interpreter interpreter, List<object?> arguments, Environment closure)
	{
		var instance = closure.GetAt(1, "this") as MorphInstance;
		var json = instance?.Get("jsonString") as JsonNode;

		return json?.ToString() ?? "";
	}

	private object? Index(Interpreter interpreter, List<object?> arguments, Environment closure)
	{
		var key = arguments[0];
		var instance = closure.GetAt(1, "this") as MorphInstance;
		var json = instance?.Get("jsonString") as JsonNode;

		if (json is null)
		{
			return null;
		}

		JsonNode? value;

		if (key is string stringKey)
		{
			value = json[stringKey];
		}
		else if (key is decimal intKey && intKey % 1 == 0)
		{
			value = json[(int)intKey];
		}
		else
		{
			throw new RuntimeException(null, "Invalid key type");
		}

		return JsonConstructor(new MorphInstance(this), value);
	}

}