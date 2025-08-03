using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class BinaryExpr : Expr
{
    public Expr Left { get; private set; }
    public Token Op { get; private set; }
    public Expr Right { get; private set; }

    public BinaryExpr(Expr left, Token op, Expr right)
    {
        Left = left;
        Op = op;
        Right = right;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   