using Morph.Parsing.Expressions;
using Morph.Parsing.Visitors;
using Morph.Scanning;

namespace Morph.Parsing.Statements;

internal record BlockStmt(List<Stmt> Statements) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record ClassDefinitionStmt(Token Name, List<FunctionDefinitionStmt> Methods) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record ExpressionStmt(Expr Expression) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record FunctionDefinitionStmt(Token Name, List<Token> Params, List<Stmt> Body) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record IfStmt(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record InStmt(Expr Type, Token Name) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record ReturnStmt(Token Keyword, Expr? Value) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record VarDefinitionStmt(Token Name, Expr? Initialiser) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}

internal record WhileStmt(Expr Condition, Stmt Body) : Stmt
{
	public override T Accept<T>(IStmtVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}