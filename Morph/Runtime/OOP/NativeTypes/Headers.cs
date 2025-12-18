using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Headers : MorphClass
{
	public Headers() : base("Headers")
	{
		var constructor = new NativeMorphConstructor(0, (_, _, _) => new HeadersInstance(this, ""));
		AddConstructor(constructor);

		constructor = new NativeMorphConstructor(1, (interp, _, args) => new HeadersInstance(this, interp.Stringify(args[0])));
		AddConstructor(constructor);
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