using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class CallExpr : Expr
{
    public Expr Callee { get; private set; }
    public Token Paren { get; private set; }
    public List<Expr> Arguments { get; private set; }

    public CallExpr(Expr callee, Token paren, List<Expr> arguments)
    {
        Callee = callee;
        Paren = paren;
        Arguments = arguments;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   