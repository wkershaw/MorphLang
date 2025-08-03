using Morph.Parsing.Expressions;
using Morph.Parsing.Visitors;
using Morph.Scanning;

namespace Morph.Parsing.Statements;

internal class VarStmt : Stmt
{
    public Token Name { get; private set; }
    public Expr? Initialiser { get; private set; }

    public VarStmt(Token name, Expr? expression)
    {
        Name = name;
        Initialiser = expression;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}