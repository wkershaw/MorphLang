using Morph.Parsing.Statements;
using Morph.Runtime.Functions.Interfaces;
using Morph.Scanning;

namespace Morph.Runtime.Functions;

internal class MorphFunction : IMorphFunction
{
    private readonly FunctionDefinitionStmt _declaration;
    private readonly Environment _closure;
	private readonly bool _isConstructor;

    public int Arity => _declaration.Params.Count;

    public MorphFunction(FunctionDefinitionStmt declaration, Environment closure)
    {
        _declaration = declaration;
        _closure = closure;
		_isConstructor = declaration.Name.Lexeme == "init";
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
            return _isConstructor ? _closure.GetAt(0, "this") : r.Value;
        }

		return _isConstructor ? _closure.GetAt(0, "this") : null;
    }

    public override string ToString()
    {
        return $"<function {_declaration.Name.Lexeme}>";
    }
}