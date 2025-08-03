using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class InterpolatedStringExpr : Expr
{
    public List<Expr> Parts { get; private set; }

    public InterpolatedStringExpr(List<Expr> parts)
    {
        Parts = parts;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   