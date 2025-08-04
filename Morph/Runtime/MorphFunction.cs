using Morph.Parsing.Statements;
using Morph.Scanning;

namespace Morph.Runtime;

internal class MorphFunction : IMorphCallable
{
    private readonly FunctionStmt _declaration;
    private readonly Environment _closure;

    public int Arity => _declaration.Params.Count;

    public MorphFunction(FunctionStmt declaration, Environment closure)
    {
        _declaration = declaration;
        _closure = closure;
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
            return r.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<function {_declaration.Name.Lexeme}>";
    }
}