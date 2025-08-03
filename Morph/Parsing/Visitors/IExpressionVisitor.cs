using Morph.Parsing.Expressions;

namespace Morph.Parsing.Visitors
{
    internal interface IExpressionVisitor<T>
    {
        T Visit(BinaryExpr expression);

        T Visit(GroupingExpr expression);

        T Visit(LiteralExpr expression);

        T Visit(UnaryExpr expression);

        T Visit(VariableExpr expression);

        T Visit(AssignExpr expression);

        T Visit(LogicalExpr expression);

        T Visit(CallExpr expression);

        T Visit(IndexExpr expression);

        T Visit(InterpolatedStringExpr expression);
    }
}