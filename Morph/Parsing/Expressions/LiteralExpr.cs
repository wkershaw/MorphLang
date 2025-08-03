using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class LiteralExpr : Expr
{
    public object? Value { get; private set; }

    public LiteralExpr(object? value)
    {
        Value = value;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   