using Morph.Scanning;

namespace Morph.Runtime.OOP;

internal class MorphInstance
{
    protected readonly MorphClass _class;
    protected readonly Dictionary<string, object?> _fields;
     
	public MorphInstance(MorphClass mClass)
    {
        _class = mClass;
        _fields = new Dictionary<string, object?>();
    }

    public object? Get(Token name)
    {
        return Get(name.Lexeme);
	}

	public object? Get(string name)
	{
		if (_fields.TryGetValue(name, out object? value))
		{
			return value;
		}

		var methodSet = _class.FindMethodSet(name);
		if (methodSet is not null)
		{
			return methodSet.Bind(this);
		}

		throw new RuntimeException(null, $"Undefined property: '{name}'");
	}

	public object? Index(object? index)
	{
		var indexMethod = _class
			.FindMethodSet("Index")?
			.Bind(this)?
			.TryFindOverload(1);

		if (indexMethod is not null)
		{
			return indexMethod.Call(new Interpreter(), new List<object?>() { index });
		}

		throw new RuntimeException(null, $"{_class.Name} does not support indexing");
	}

	public void Set(Token name, object? value)
    {
        _fields[name.Lexeme] = value;
    }

	public void Set(string name, object? value)
	{
		_fields[name] = value;
	}

	public string ToString(Interpreter interpreter)
	{
		var toStringMethod = _class
			.FindMethodSet("ToString")?
			.Bind(this)?
			.TryFindOverload(0);

		if (toStringMethod is null)
		{
			return _class.Name + " instance";
		}

		var result = toStringMethod.Call(interpreter, new List<object?>());
		return interpreter.Stringify(result);
	}

    public override string ToString()
    {
		return _class.Name + " instance";
    }
}