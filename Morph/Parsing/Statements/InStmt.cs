using Morph.Scanning;
using Morph.Parsing.Visitors;
using Morph.Parsing.Expressions;

namespace Morph.Parsing.Statements;

internal class InStmt : Stmt
{
    public Expr Type { get; private set; }
    public Token Name { get; private set; }

    public InStmt(Expr type, Token name)
    {
        Type = type;
        Name = name;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   