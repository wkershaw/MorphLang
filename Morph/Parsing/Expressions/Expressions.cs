using Morph.Parsing.Visitors;
using Morph.Scanning;

namespace Morph.Parsing.Expressions;

internal record AssignExpr(Token Name, Expr Value) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record BinaryExpr(Expr Left, Token Op, Expr Right) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record CallExpr(Expr Callee, Token Paren, List<Expr> Arguments) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record GetExpr(Expr Object, Token Name) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record GroupingExpr(Expr Expression) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record IndexExpr(Expr Callee, Token Bracket, Expr Index) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record InterpolatedStringExpr(List<Expr> Parts) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record LiteralExpr(object? Value) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record LogicalExpr(Expr Left, Token Op, Expr Right) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record SetExpr(Expr Object, Token Name, Expr Value) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record ThisExpr(Token Keyword) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record UnaryExpr(Token Op, Expr Right) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record VariableExpr(Token Name) : Expr
{
	public override T Accept<T>(IExprVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
