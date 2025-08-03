using Morph.Parsing.Expressions;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Statements;

internal class ExpressionStmt : Stmt
{
    public Expr Expression { get; private set; }

    public ExpressionStmt(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}