namespace Morph.Runtime.NativeTypes;

internal class Headers : MorphClass
{
	public Headers() : base("Headers", new Dictionary<string, IMorphInstanceCallable>())
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
			return new HeadersInstance(_class, arguments[0]?.ToString() ?? "");
		}
	}

	public class HeadersInstance : MorphInstance, IMorphIndexable
	{
		private readonly Dictionary<string, string> _headers;

		public HeadersInstance(MorphClass mClass, string headersString) : base(mClass)
		{
			_headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var headerStrings = headersString.Split(';');

			foreach (var headerString in headerStrings)
			{
				var parts = headerString.Split(':', 2);
				if (parts.Length == 2)
				{
					var key = parts[0].Trim();
					var value = parts[1].Trim();
					_headers[key] = value;
				}
			}
		}

		object? IMorphIndexable.Get(Interpreter interpreter, object? key)
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
		public override string ToString()
		{
			return string.Join("; ", _headers.Select(kv => $"{kv.Key}: {kv.Value}"));
		}

	}
}