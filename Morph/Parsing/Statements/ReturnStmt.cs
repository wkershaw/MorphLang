using Morph.Scanning;
using Morph.Parsing.Visitors;
using Morph.Parsing.Expressions;

namespace Morph.Parsing.Statements;

internal class ReturnStmt : Stmt
{
    public Token Keyword { get; private set; }
    public Expr? Value { get; private set; }

    public ReturnStmt(Token keyword, Expr? value)
    {
        Keyword = keyword;
        Value = value;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   