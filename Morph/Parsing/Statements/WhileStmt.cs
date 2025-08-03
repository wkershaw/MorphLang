using Morph.Parsing.Visitors;
using Morph.Parsing.Expressions;

namespace Morph.Parsing.Statements;

internal class WhileStmt : Stmt
{
    public Expr Condition { get; private set; }
    public Stmt Body { get; private set; }

    public WhileStmt(Expr condition, Stmt body)
    {
        Condition = condition;
        Body = body;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   