using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class ThisExpr : Expr
{
    public Token Keyword { get; private set; }

    public ThisExpr(Token keyword)
    {
        Keyword = keyword;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   