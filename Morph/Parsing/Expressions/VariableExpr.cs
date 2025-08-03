using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class VariableExpr : Expr
{
    public Token Name { get; private set; }

    public VariableExpr(Token name)
    {
        Name = name;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   