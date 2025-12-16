using Morph.Parsing.Visitors;

namespace Morph.Parsing.Statements;

internal abstract record Stmt
{
    public abstract T Accept<T>(IStmtVisitor<T> visitor);
}