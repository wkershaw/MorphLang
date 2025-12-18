using Morph.Parsing.Statements;
using Morph.Runtime.Functions;
using Morph.Runtime.Functions.Interfaces;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP
{
    internal class MorphMethod : IMorphMethod
    {
        private readonly FunctionDefinitionStmt _declaration;
        private readonly Environment _closure;

        public int Arity => _declaration.Params.Count;

        public MorphMethod(FunctionDefinitionStmt declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public IMorphFunction Bind(MorphInstance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);
            return new MorphFunction(_declaration, environment);
        }

        public override string ToString()
        {
            return $"<method {_declaration.Name.Lexeme}>";
        }
    }
}
