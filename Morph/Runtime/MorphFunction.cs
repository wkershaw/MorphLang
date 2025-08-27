using Morph.Parsing.Statements;
using Morph.Scanning;

namespace Morph.Runtime;

internal class MorphFunction : IMorphInstanceCallable
{
    private readonly FunctionStmt _declaration;
    private readonly Environment _closure;
    private bool _isInitialiser;

    public int Arity => _declaration.Params.Count;

    public MorphFunction(FunctionStmt declaration, Environment closure, bool isInitialiser)
    {
        _declaration = declaration;
        _closure = closure;
        _isInitialiser = isInitialiser;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment environment = new Environment(_closure);

        foreach ((Token param, object? argument) in _declaration.Params.Zip(arguments))
        {
            environment.Define(param.Lexeme, argument);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return r)
        {
            if (_isInitialiser)
            {
                return _closure.GetAt(0, "this");
            }

            return r.Value;
        }

        if (_isInitialiser)
        {
            return _closure.GetAt(0, "this");
        }

        return null;
    }

    public IMorphCallable Bind(MorphInstance instance)
    {
        Environment environment = new Environment(_closure);
        environment.Define("this", instance);
        return new MorphFunction(_declaration, environment, _isInitialiser);
    }

    public override string ToString()
    {
        return $"<function {_declaration.Name.Lexeme}>";
    }
}