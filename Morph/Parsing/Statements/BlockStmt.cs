using Morph.Parsing.Visitors;

namespace Morph.Parsing.Statements;

internal class BlockStmt : Stmt
{
    public List<Stmt> Statements { get; private set; }

    public BlockStmt(List<Stmt> statements)
    {
        Statements = statements;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}