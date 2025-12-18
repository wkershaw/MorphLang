using Morph.Parsing.Statements;
using Morph.Runtime.OOP.Interfaces;
using Morph.Scanning;
namespace Morph.Runtime.OOP
{
    internal class MorphConstructor : IMorphConstructor
    {
        private readonly FunctionDefinitionStmt _declaration;
        private readonly Environment _closure;

        public int Arity => _declaration.Params.Count;

        public MorphConstructor(FunctionDefinitionStmt declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public MorphInstance Call(Interpreter interpreter, MorphInstance instance, List<object?> arguments)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);

            foreach ((Token param, object? argument) in _declaration.Params.Zip(arguments))
            {
                environment.Define(param.Lexeme, argument);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return)
            {
                return instance;
            }

            return instance;
        }

        public override string ToString()
        {
            return $"<constructor {_declaration.Name.Lexeme}>";
        }

    }
}
