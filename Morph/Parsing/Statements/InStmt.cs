using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Statements;

internal class InStmt : Stmt
{
    public Token Type { get; private set; }
    public Token Name { get; private set; }

    public InStmt(Token type, Token name)
    {
        Type = type;
        Name = name;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   