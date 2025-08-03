using Morph.Parsing.Visitors;

namespace Morph.Parsing.Expressions;

internal abstract class Expr
{
    public abstract T Accept<T>(IExpressionVisitor<T> visitor);
}