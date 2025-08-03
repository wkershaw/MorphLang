using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class GroupingExpr : Expr
{
    public Expr Expression { get; private set; }

    public GroupingExpr(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   