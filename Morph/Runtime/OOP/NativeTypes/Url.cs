using Morph.Runtime.OOP.Native;
using System.Web;

namespace Morph.Runtime.OOP.NativeTypes;

internal class Url : MorphClass
{
	public Url() : base("Url")
	{
		var constructor = new NativeMorphConstructor(1, (interp, _, args) => new UrlInstance(this, interp.Stringify(args[0])));
		AddConstructor(constructor);
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

		public object? Get(Interpreter interpreter, object? key)
		{
			if (key is string keyString)
			{
				var queryString = HttpUtility.ParseQueryString(_uri.Query);
				return queryString[keyString];
			}

			if (int.TryParse(key?.ToString(), out int index))
			{
				var segments = _uri.Segments;
				if (index < 0)
				{
					index = segments.Length + index;
				}

				if (index >= 0 && index < segments.Length)
				{
					return segments[index].Trim('/');
				}

				return null;
			}

			throw new RuntimeException(null, "Can only index Url with a string or int");
		}

		public override string ToString()
		{
			return _uri.OriginalString;
		}
	}
}