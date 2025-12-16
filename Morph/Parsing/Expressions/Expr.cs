using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal abstract record Expr
{
    public abstract T Accept<T>(IExprVisitor<T> visitor);
}