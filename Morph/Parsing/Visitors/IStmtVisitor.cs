using Morph.Parsing.Statements;

namespace Morph.Parsing.Visitors;

internal interface IStmtVisitor<T>
{
    T Visit(FunctionStmt statement);

    T Visit(VarStmt statement);

    T Visit(InStmt statement);

    T Visit(ExpressionStmt statement);

    T Visit(IfStmt statement);

    T Visit(ReturnStmt statement);

    T Visit(WhileStmt statement);

    T Visit(BlockStmt statement);
}