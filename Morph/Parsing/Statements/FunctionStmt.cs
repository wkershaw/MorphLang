using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Statements;

internal class FunctionStmt : Stmt
{
    public Token Name { get; private set; }
    public List<Token> Params { get; private set; }
    public List<Stmt> Body { get; private set; }

    public FunctionStmt(Token name, List<Token> parameters, List<Stmt> body)
    {
        Name = name;
        Params = parameters;
        Body = body;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   