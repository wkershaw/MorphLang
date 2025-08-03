using Morph.Parsing.Statements;
using Morph.Scanning;

namespace Morph.Runtime;

internal class MorphFunction : IMorphCallable
{
    private readonly FunctionStmt _declaration;

    public int Arity => _declaration.Params.Count;

    public MorphFunction(FunctionStmt declaration)
    {
        _declaration = declaration;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment environment = new Environment(interpreter.Globals);

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