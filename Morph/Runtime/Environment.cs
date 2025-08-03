using Morph.Scanning;

namespace Morph.Runtime;

internal class Environment
{
    private readonly Environment? _enclosing;
    private readonly Dictionary<string, object?> _values = new();

    public Environment()
    {
        _enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        _enclosing = enclosing;
    }

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (_enclosing != null)
        {
            return _enclosing.Get(name);
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
    
    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (_enclosing != null)
        {
            _enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}