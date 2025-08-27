using Morph.Scanning;

namespace Morph.Runtime;

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
        if (_fields.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        var method = _class.FindMethod(name.Lexeme);
        if (method is not null)
        {
            return method.Bind(this);
        }

        throw new RuntimeException(name, $"Undefined property: '{name.Lexeme}'");
    }

    public void Set(Token name, object? value)
    {
        _fields[name.Lexeme] = value;
    }

    public override string ToString()
    {
        return _class.Name + " instance";
    }
}