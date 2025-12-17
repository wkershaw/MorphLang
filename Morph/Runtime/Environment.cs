using Morph.Scanning;

namespace Morph.Runtime;

internal class Environment
{
    private readonly Environment? _parent;
    private readonly Dictionary<string, object?> _values;

    public Environment(Environment? parent = null)
    {
        _parent = parent;
		_values = new Dictionary<string, object?>();
	}

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? Get(Token name)
    {
		return Get(name.Lexeme);
    }

	public object? Get(string name)
	{
		if (_values.TryGetValue(name, out var value))
		{
			return value;
		}

		if (_parent != null)
		{
			return _parent.Get(name);
		}

		throw new RuntimeException(null, $"Undefined variable '{name}'.");
	}

	public void AssignAt(int distance, Token name, object? value)
	{
		Ancestor(distance)._values[name.Lexeme] = value;
	}

	public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)._values[name];
    }

    private Environment Ancestor(int distance)
    {
        Environment env = this;
        for (int i = 0; i < distance; i++)
        {
            env = env._parent!;
        }

        return env;
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (_parent != null)
        {
            _parent.Assign(name, value);
            return;
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}