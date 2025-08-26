
namespace Morph.Runtime;

internal class MorphClass : IMorphCallable
{
    private readonly Dictionary<string, MorphFunction> _methods;
    public int Arity => CalculateArity();
    public string Name { get; private init; }

    public MorphClass(string name, Dictionary<string, MorphFunction> methods)
    {
        _methods = methods;
        Name = name;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new MorphInstance(this);

        MorphFunction? initialiser = FindMethod("init");
        if (initialiser is not null)
        {
            initialiser.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public MorphFunction? FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out MorphFunction? method))
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
        MorphFunction? initialiser = FindMethod("init");
        if (initialiser is not null)
        {
            return initialiser.Arity;
        }

        return 0;
    }
}