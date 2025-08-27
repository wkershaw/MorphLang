
namespace Morph.Runtime;

internal class MorphClass : IMorphCallable
{
    protected readonly Dictionary<string, IMorphInstanceCallable> _methods;
    public int Arity => CalculateArity();
    public string Name { get; private init; }

    public MorphClass(string name, Dictionary<string, IMorphInstanceCallable> methods)
    {
        _methods = methods;
        Name = name;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new MorphInstance(this);

		IMorphInstanceCallable? initialiser = FindMethod("init");
        if (initialiser is not null)
        {
            return initialiser.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public IMorphInstanceCallable? FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out IMorphInstanceCallable? method))
        {
            return method;
        }

        return null;
    }

    public override string ToString()
    {
        return Name;
    }

    private int CalculateArity()
    {
		IMorphInstanceCallable? initialiser = FindMethod("init");
        if (initialiser is not null)
        {
            return initialiser.Arity;
        }

        return 0;
    }
}