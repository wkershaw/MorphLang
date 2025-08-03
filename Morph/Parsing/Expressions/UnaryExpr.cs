using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class UnaryExpr : Expr
{
    public Token Op { get; private set; }
    public Expr Right { get; private set; }

    public UnaryExpr(Token op, Expr right)
    {
        Op = op;
        Right = right;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   