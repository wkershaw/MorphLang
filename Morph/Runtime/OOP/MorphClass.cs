using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP;

internal class MorphClass
{
    protected readonly Dictionary<string, MethodSet> _methods;

	protected readonly Dictionary<int, IMorphConstructor> _constructors;

    public string Name { get; private init; }

    public MorphClass(string name)
    {
        Name = name;
        _methods = new Dictionary<string, MethodSet>();
        _constructors = new Dictionary<int, IMorphConstructor>();
    }

    public MorphInstance Construct(Interpreter interpreter, List<object?> arguments)
    {
        if (!_constructors.TryGetValue(arguments.Count, out IMorphConstructor? constructor))
		{
			if (_constructors.Count == 0 && arguments.Count == 0)
			{
				// No constructors defined and no args passed
				// so use an empty constructor
				return new MorphInstance(this);
			}

			throw new RuntimeException(null, $"No constructor found for class {Name} with {arguments.Count} arguments.");
		}

		var instance = new MorphInstance(this);
		return constructor.Call(interpreter, instance, arguments);
    }

    public MethodSet? FindMethodSet(string name)
    {
        if (_methods.TryGetValue(name, out MethodSet? methodSet))
        {
            return methodSet;
        }

        return null;
    }

	public void AddMethod(string name, IMorphMethod method)
	{
		if (_methods.TryGetValue(name, out MethodSet? methodSet))
		{
			methodSet.AddOverload(method);
			return;
		}

		methodSet = new MethodSet(name);
		methodSet.AddOverload(method);

		_methods.Add(name, methodSet);
	}

	public void AddConstructor(IMorphConstructor constructor)
	{
		if (_constructors.ContainsKey(constructor.Arity))
		{
			throw new RuntimeException(null, $"Constructor with {constructor.Arity} parameters is already defined in class {Name}.");
		}

		_constructors.Add(constructor.Arity, constructor);
	}

	public override string ToString()
    {
        return $"<class {Name}>";
    }
}