using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class IndexExpr : Expr
{
    public Expr Callee { get; private set; }
    public Token Bracket { get; private set; }
    public Expr Index { get; private set; }

    public IndexExpr(Expr callee, Token bracket, Expr index)
    {
        Callee = callee;
        Bracket = bracket;
        Index = index;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   