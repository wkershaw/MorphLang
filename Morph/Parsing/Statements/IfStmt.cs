using Morph.Parsing.Visitors;
using Morph.Parsing.Expressions;

namespace Morph.Parsing.Statements;

internal class IfStmt : Stmt
{
    public Expr Condition { get; private set; }
    public Stmt ThenBranch { get; private set; }
    public Stmt? ElseBranch { get; private set; }

    public IfStmt(Expr condition, Stmt thenbranch, Stmt? elsebranch)
    {
        Condition = condition;
        ThenBranch = thenbranch;
        ElseBranch = elsebranch;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   