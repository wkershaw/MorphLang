using Morph.Parsing.Visitors;
using Morph.Scanning;

namespace Morph.Parsing.Expressions;

internal class GetExpr : Expr
{
    public Expr Object { get; private set; }
    public Token Name { get; private set; }

    public GetExpr(Expr obj, Token name)
    {
        Object = obj;
        Name = name;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   