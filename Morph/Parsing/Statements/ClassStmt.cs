using Morph.Scanning;
using Morph.Parsing.Visitors;

namespace Morph.Parsing.Statements;

internal class ClassStmt : Stmt
{
    public Token Name { get; private set; }
    public List<FunctionStmt> Methods { get; private set; }

    public ClassStmt(Token name, List<FunctionStmt> methods)
    {
        Name = name;
        Methods = methods;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}   