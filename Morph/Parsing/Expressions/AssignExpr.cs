using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal class AssignExpr : Expr
{
    public Token Name { get; private set; }
    public Expr Value { get; private set; }

    public AssignExpr(Token name, Expr value)
    {
        Name = name;
        Value = value;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   