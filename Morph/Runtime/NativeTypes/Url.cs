using System.Web;

namespace Morph.Runtime.NativeTypes;

internal class Url : MorphClass
{
	public Url() : base("Url", new Dictionary<string, IMorphInstanceCallable>())
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
			return new UrlInstance(_class, arguments[0]?.ToString() ?? "");
		}
	}

	public class UrlInstance : MorphInstance, IMorphIndexable
	{
		private readonly Uri _uri;

		public UrlInstance(MorphClass mClass, string urlString) : base(mClass)
		{
			if (Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uri))
			{
				_uri = uri;
				_fields.Add("urlString", urlString.Trim('/'));
				_fields.Add("path", uri.GetLeftPart(UriPartial.Path).Trim('/'));
				_fields.Add("localPath", uri.LocalPath.Trim('/'));
				_fields.Add("query", uri.Query.TrimStart('?'));
			}
			else
			{
				throw new RuntimeException(null, $"Provided string is not a valid URL: {urlString}");
			}
		}

		object? IMorphIndexable.Get(Interpreter interpreter, object? key)
		{
			if (key is not string keyString)
			{
				throw new RuntimeException(null, "Can only index Url with a string");
			}

			var queryString = HttpUtility.ParseQueryString(_uri.Query);
			return queryString[keyString];
		}
		public override string ToString()
		{
			return _uri.OriginalString;
		}
	}
}