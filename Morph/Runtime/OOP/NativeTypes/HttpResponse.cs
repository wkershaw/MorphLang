using Morph.Runtime.OOP.Native;

namespace Morph.Runtime.OOP.NativeTypes;

internal class HttpResponse : MorphClass
{
	public HttpResponse(Environment closure) : base("HttpResponse")
	{
		var constructor = new NativeMorphConstructor(0, Constructor, closure);
		AddConstructor(constructor);

		var toString = new NativeMorphMethod(0, ToString, closure);
		AddMethod("ToString", toString);
	}

	private MorphInstance Constructor(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
	{
		instance.Set("statusCode", 200);
		instance.Set("body", "");

		return instance;
	}

	private object? ToString(Interpreter interpreter, List<object?> arguments, Environment closure)
	{
		var instance = closure.GetAt(1, "this") as MorphInstance;
		var statusCode = instance?.Get("statusCode");

		return "HTTP Response ToString() overload! Status code: " + interpreter.Stringify(statusCode);
	}
}