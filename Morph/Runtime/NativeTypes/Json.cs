using System.Text.Json;
using System.Text.Json.Nodes;

namespace Morph.Runtime.NativeTypes;

internal class Json : MorphClass
{
	public Json() : base("Json", new Dictionary<string, IMorphInstanceCallable>())
	{
		foreach (var method in GetMethods())
		{
			_methods.Add(method.Key, method.Value);
		}
	}

	private Dictionary<string, IMorphInstanceCallable> GetMethods()
	{
		var methods = new Dictionary<string, IMorphInstanceCallable>();

		methods.Add("init", new Constructor(this));

		return methods;
	}

	public class Constructor : IMorphInstanceCallable
	{
		public int Arity => 1;

		private MorphClass _class;

		public Constructor(MorphClass c)
		{
			_class = c;
		}

		IMorphCallable IMorphInstanceCallable.Bind(MorphInstance instance)
		{
			return this;
		}

		object? IMorphCallable.Call(Interpreter interpreter, List<object?> arguments)
		{
			return new JsonInstance(_class, arguments[0]?.ToString() ?? "");
		}
	}

	public class JsonInstance : MorphInstance, IMorphIndexable
	{
		private readonly JsonNode? _json;

		public JsonInstance(MorphClass mClass, string jsonString) : base(mClass)
		{
			_json = JsonSerializer.Deserialize<JsonNode>(jsonString)
				?? throw new RuntimeException(null, "Could not parse JSON");
		}

		public JsonInstance(MorphClass mClass, JsonNode? json) : base(mClass)
		{
			_json = json;
		}

		object? IMorphIndexable.Get(Interpreter interpreter, object? key)
		{
			if (_json is null)
			{
				return null;
			}

			JsonNode? value = null;

			if (key is string stringKey)
			{
				value = _json[stringKey];
			}
			else if (key is decimal intKey && intKey % 1 == 0)
			{
				value = _json[(int)intKey];
			}
			else
			{
				throw new RuntimeException(null, "Invalid key type");
			}

			return new JsonInstance(_class, value);
		}

		public override string ToString()
		{
			return _json?.ToString() ?? "";
		}

	}
}
