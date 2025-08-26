using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class SetExpr : Expr
{
    public Expr Object { get; private set; }
    public Token Name { get; private set; }
    public Expr Value { get; private set; }

    public SetExpr(Expr obj, Token name, Expr value)
    {
        Object = obj;
        Name = name;
        Value = value;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   