using Morph.Runtime.OOP.Native;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Json : MorphClass
{
	public Json() : base("Json")
	{
		var constructor = new NativeMorphConstructor(0, (_, _, _) => new JsonInstance(this, ""));
		AddConstructor(constructor);

		constructor = new NativeMorphConstructor(1, (interp, _, args) => new JsonInstance(this, interp.Stringify(args[0])));
		AddConstructor(constructor);
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

            JsonNode? value;

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