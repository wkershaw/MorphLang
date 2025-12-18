using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;
/*
internal class Headers : MorphClass
{
	public Headers(Environment closure) : base("Headers")
	{
		var constructor = new NativeMorphConstructor(1, Constructor, closure);
		AddConstructor(constructor);

		var toString = new NativeMorphMethod(0, ToString, closure);
		AddMethod("ToString", toString);

		var index = new NativeMorphMethod(0, Index, closure);
		AddMethod("Index", index);
	}


	private MorphInstance Constructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
	{
		string headersString = interpreter.Stringify(arguments[0]);
		var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var headerStrings = headersString.Split(';');

		foreach (var headerString in headerStrings)
		{
			var parts = headerString.Split(':', 2);
			if (parts.Length == 2)
			{
				var key = parts[0].Trim();
				var value = parts[1].Trim();
				headers[key] = value;
			}
		}

		instance.Set("headers", headers);

		return instance;
	}

	object? Index(Interpreter interpreter, object? key, Environment closure)
	{
		if (key is string strKey)
		{
			if (_headers.TryGetValue(strKey, out var value))
			{
				return value;
			}
			return null;
		}

		throw new RuntimeException(null, "Headers keys must be strings.");
	}

	private object? ToString(Interpreter interpreter, List<object?> Arguments, Environment closure)
	{
		return string.Join("; ", _headers.Select(kv => $"{kv.Key}: {kv.Value}"));
	}
}

*/