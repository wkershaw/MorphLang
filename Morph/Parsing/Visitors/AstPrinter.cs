using System.Text;
using Morph.Parsing.Expressions;

namespace Morph.Parsing.Visitors;

internal class AstPrinter : IExpressionVisitor<string>
{
    public string Print(Expr epxr)
    {
        return epxr.Accept(this);
    }

    public string Visit(BinaryExpr expression)
    {
        return Parenthesize(expression.Op.Lexeme, expression.Left, expression.Right);
    }

    public string Visit(GroupingExpr expression)
    {
        return Parenthesize("group", expression.Expression);
    }

    public string Visit(LiteralExpr expression)
    {
        return expression.Value?.ToString() ?? "nil";
    }

    public string Visit(UnaryExpr expression)
    {
        return Parenthesize(expression.Op.Lexeme, expression.Right);
    }

    public string Visit(VariableExpr expression)
    {
        return expression.Name.Lexeme;
    }

    public string Visit(AssignExpr expression)
    {
        return Parenthesize("=", expression);
    }

    public string Visit(LogicalExpr expression)
    {
        return Parenthesize(expression.Op.Lexeme, expression.Left, expression.Right);
    }

    public string Visit(CallExpr expression)
    {
        var args = string.Join(", ", expression.Arguments.Select(arg => arg.Accept(this)));
        return Parenthesize($"{expression.Callee.Accept(this)}({args})");
    }

    public string Visit(IndexExpr expression)
    {
        return Parenthesize($"{expression.Callee.Accept(this)}[{expression.Index.Accept(this)}]", expression.Callee, expression.Index);
    }

    public string Visit(InterpolatedStringExpr expression)
    {
        return Parenthesize("Interpolated string", expression.Parts.ToArray());
    }

    private string Parenthesize(string name, params Expr[] expressions)
    {
        var sb = new StringBuilder();
        sb
        .Append("(")
        .Append(name);

        foreach (var expr in expressions)
        {
            sb.Append(" ").Append(expr.Accept(this));
        }

        sb.Append(")");
        return sb.ToString();
    }
}