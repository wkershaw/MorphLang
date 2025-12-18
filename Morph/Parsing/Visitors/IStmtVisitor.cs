using Morph.Parsing.Statements;

namespace Morph.Parsing.Visitors;

internal interface IStmtVisitor<T>
{
    T Visit(FunctionDefinitionStmt statement);

    T Visit(VarDefinitionStmt statement);

    T Visit(InStmt statement);

    T Visit(ExpressionStmt statement);

    T Visit(IfStmt statement);

    T Visit(ReturnStmt statement);

    T Visit(WhileStmt statement);

    T Visit(BlockStmt statement);

    T Visit(ClassDefinitionStmt statement);
}