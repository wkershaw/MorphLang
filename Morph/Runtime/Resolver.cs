using Morph.Parsing.Expressions;
using Morph.Parsing.Statements;
using Morph.Parsing.Visitors;
using Morph.Scanning;

namespace Morph.Runtime;

internal class Resolver : IExpressionVisitor<object?>, IStmtVisitor<object?>
{
    private enum FunctionType
    {
        None,
        Function,
        Initialiser,
        Method,
    }

    private enum ClassType
    {
        None,
        Class
    }

    private ClassType _currentClass = ClassType.None;

    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, bool>> _scopes;
    private FunctionType _currentFunction = FunctionType.None;

    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
        _scopes = new Stack<Dictionary<string, bool>>();
    }

    public void Resolve(IEnumerable<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    public object? Visit(BlockStmt statement)
    {
        BeginScope();
        Resolve(statement.Statements);
        EndScope();

        return null;
    }

    public object? Visit(VarStmt statement)
    {
        Declare(statement.Name);

        if (statement.Initialiser != null)
        {
            Resolve(statement.Initialiser);
        }

        Define(statement.Name);
        return null;
    }

    public object? Visit(FunctionStmt statement)
    {
        Declare(statement.Name);
        Define(statement.Name);

        ResolveFunction(statement, FunctionType.Function);
        return null;
    }

    public object? Visit(ClassStmt statement)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;

        Declare(statement.Name);
        Define(statement.Name);

        BeginScope();
        _scopes.Peek().Add("this", true);

        foreach (FunctionStmt method in statement.Methods)
        {
            var declaration = FunctionType.Method;
            if (method.Name.Lexeme == "init")
            {
                declaration = FunctionType.Initialiser;
            }

            ResolveFunction(method, declaration);
        }

        EndScope();

        _currentClass = enclosingClass;
        return null;
    }

    public object? Visit(ExpressionStmt statement)
    {
        Resolve(statement.Expression);
        return null;
    }

    public object? Visit(IfStmt statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.ThenBranch);

        if (statement.ElseBranch is not null)
        {
            Resolve(statement.ElseBranch);
        }

        return null;
    }

    public object? Visit(ReturnStmt statement)
    {
        if (statement.Value is not null)
        {
            if (_currentFunction == FunctionType.None)
            {
                Morph.Error(statement.Keyword, "Can't return a value from top level code");
            }
            
            if (_currentFunction == FunctionType.Initialiser)
            {
                Morph.Error(statement.Keyword, "Can't return a value from an initialiser");
            }

            Resolve(statement.Value);
        }

        return null;
    }

    public object? Visit(WhileStmt statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.Body);
        return null;
    }

    public object? Visit(InStmt statement)
    {
        Declare(statement.Name);
        Define(statement.Name);
        return null;
    }

    public object? Visit(VariableExpr expression)
    {
        if (_scopes.Any()
            && _scopes.Peek().TryGetValue(expression.Name.Lexeme, out bool finished)
            && finished == false)
        {
            Morph.Error(expression.Name, "Can't read local variable in its own initialiser");
        }

        ResolveLocal(expression, expression.Name);
        return null;
    }

    public object? Visit(AssignExpr expression)
    {
        Resolve(expression.Value);
        ResolveLocal(expression, expression.Name);
        return null;
    }

    public object? Visit(BinaryExpr expession)
    {
        Resolve(expession.Left);
        Resolve(expession.Right);
        return null;
    }

    public object? Visit(CallExpr expession)
    {
        Resolve(expession.Callee);

        foreach (Expr argument in expession.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? Visit(GetExpr expression)
    {
        Resolve(expression.Object);
        return null;
    }

    public object? Visit(SetExpr expression)
    {
        Resolve(expression.Value);
        Resolve(expression.Object);
        return null;
    }

    public object? Visit(ThisExpr expression)
    {
        if (_currentClass == ClassType.None)
        {
            Morph.Error(expression.Keyword, "Can't use 'this' outside of a class");
            return null;
        }

        ResolveLocal(expression, expression.Keyword);
        return null;
    }

    public object? Visit(GroupingExpr expession)
    {
        Resolve(expession.Expression);
        return null;
    }

    public object? Visit(LiteralExpr _)
    {
        return null;
    }

    public object? Visit(LogicalExpr expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);
        return null;
    }

    public object? Visit(UnaryExpr expression)
    {
        Resolve(expression.Right);
        return null;
    }

    public object? Visit(IndexExpr expression)
    {
        Resolve(expression.Callee);
        Resolve(expression.Index);
        return null;
    }

    public object? Visit(InterpolatedStringExpr expression)
    {
        foreach (Expr part in expression.Parts)
        {
            Resolve(part);
        }

        return null;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count() == 0)
        {
            return;
        }

        Dictionary<string, bool> scope = _scopes.Peek();

        if (scope.ContainsKey(name.Lexeme))
        {
            Morph.Error(name, "Already a variable with this name in scope");
            return;
        }

        scope.Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (_scopes.Count() == 0)
        {
            return;
        }

        Dictionary<string, bool> scope = _scopes.Peek();
        scope[name.Lexeme] = true;
    }

    private void ResolveFunction(FunctionStmt statement, FunctionType type)
    {
        FunctionType enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope();
        foreach (Token param in statement.Params)
        {
            Declare(param);
            Define(param);
        }

        Resolve(statement.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    private void ResolveLocal(Expr expression, Token name)
    {
		for (int i = 0; i < _scopes.Count(); i++)
		{
			if (_scopes.ElementAt(i).ContainsKey(name.Lexeme))
			{
				_interpreter.Resolve(expression, i);
				return;
			}
		}
    }

    private void Resolve(Stmt statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expr expression)
    {
        expression.Accept(this);
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }
}