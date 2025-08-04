using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Morph.Parsing.Expressions;
using Morph.Parsing.Statements;
using Morph.Parsing.Visitors;
using Morph.Runtime.NativeFunctions;
using Morph.Scanning;

namespace Morph.Runtime;

internal class Interpreter : IExpressionVisitor<object?>, IStmtVisitor<object?>
{
    private Dictionary<string, string> _inputs;
    public Dictionary<string, object?> ParsedInputs { get; private set; }

    public Environment Globals { get; private set; }
    private Environment _environment;
    private readonly Dictionary<Expr, int> _locals;

    public Interpreter()
    {
        _inputs = new Dictionary<string, string>();
        ParsedInputs = new Dictionary<string, object?>();

        Globals = new Environment();

        Globals.Define("Clock", new ClockCallable());
        Globals.Define("Write", new WriteCallable());
        Globals.Define("Debug", new DebugCallable());
        Globals.Define("WriteLine", new WriteLineCallable());

        _environment = Globals;
        _locals = new Dictionary<Expr, int>();
    }

    public void Interpret(List<Stmt> statements, Dictionary<string, string> inputs)
    {
        _inputs = inputs;
        Interpret(statements);
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeException e)
        {
            Morph.RuntimeError(e);
        }
    }

    public string Stringify(object? obj)
    {
        if (obj is null) return "nil";
        if (obj is decimal dec) return dec.ToString();
        if (obj is string str) return str;
        return obj.ToString() ?? "nil";
    }

    public void Resolve(Expr expr, int depth)
    {
        _locals.Add(expr, depth);   
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = _environment;
        try
        {
            _environment = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public object? Visit(FunctionStmt statement)
    {
        var function = new MorphFunction(statement, _environment);
        _environment.Define(statement.Name.Lexeme, function);

        return null;
    }

    public object? Visit(VarStmt statement)
    {
        object? value = null;
        if (statement.Initialiser != null)
        {
            value = Evaluate(statement.Initialiser);
        }

        _environment.Define(statement.Name.Lexeme, value);
        return null;
    }

    public object? Visit(InStmt statement)
    {
        IMorphIndexable input = statement.Type.Type switch
        {
            TokenType.Json => JsonInput(statement),
            TokenType.Url => UrlInput(statement),
            _ => throw new RuntimeException(statement.Type, "Unsupported input type.")
        };

        _environment.Define(statement.Name.Lexeme, input);
        return null;
    }

    private MorphJsonInput JsonInput(InStmt statement)
    {
        if (!_inputs.TryGetValue(statement.Name.Lexeme, out string? jsonString))
        {
            throw new RuntimeException(statement.Name, $"Input '{statement.Name.Lexeme}' not found.");
        }

        var json = JsonSerializer.Deserialize<JsonObject>(jsonString);
        ParsedInputs[statement.Name.Lexeme] = json;

        return new MorphJsonInput(this, statement);
    }

    private MorphUrlInput UrlInput(InStmt statement)
    {
        if (!_inputs.TryGetValue(statement.Name.Lexeme, out string? urlString))
        {
            throw new RuntimeException(statement.Name, $"Input '{statement.Name.Lexeme}' not found");
        }

        ParsedInputs[statement.Name.Lexeme] = urlString;

        return new MorphUrlInput(this, statement);
    }

    public object? Visit(ExpressionStmt statement)
    {
        Evaluate(statement.Expression);
        return null;
    }

    public object? Visit(IfStmt statement)
    {
        if (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.ThenBranch);
        }
        else if (statement.ElseBranch != null)
        {
            Execute(statement.ElseBranch);
        }
        return null;
    }

    public object? Visit(ReturnStmt statement)
    {
        object? value = null;
        if (statement.Value != null)
        {
            value = Evaluate(statement.Value);
        }

        throw new Return(value);
    }

    public object? Visit(WhileStmt statement)
    {
        while (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.Body);
        }
        return null;
    }

    public object? Visit(BlockStmt statement)
    {
        ExecuteBlock(statement.Statements, new Environment(_environment));
        return null;
    }

    public object? Visit(GroupingExpr expression)
    {
        return Evaluate(expression.Expression);
    }

    public object? Visit(VariableExpr expression)
    {
        return LookUpVariable(expression.Name, expression);
    }

    public object? Visit(AssignExpr expression)
    {
        object? value = Evaluate(expression.Value);

        if (_locals.TryGetValue(expression, out int distance))
        {
            _environment.AssignAt(distance, expression.Name, value);
        }
        else
        {
            Globals.Assign(expression.Name, value);
        }

        return value;
    }

    public object? Visit(LogicalExpr expression)
    {
        object? left = Evaluate(expression.Left);

        if (expression.Op.Type == TokenType.Or && IsTruthy(left))
        {
            return left;
        }

        if (expression.Op.Type == TokenType.And && !IsTruthy(left))
        {
            return left;
        }

        return Evaluate(expression.Right);
    }

    public object? Visit(BinaryExpr expression)
    {
        object? left = Evaluate(expression.Left);
        object? right = Evaluate(expression.Right);

        decimal leftValue, rightValue;

        switch (expression.Op.Type)
        {
            case TokenType.Plus:
                if (left is decimal leftDecimal && right is decimal rightDecimal)
                {
                    return leftDecimal + rightDecimal;
                }

                return (left?.ToString() ?? "nil") + (right?.ToString() ?? "nil");

            case TokenType.Minus:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue - rightValue;

            case TokenType.Slash:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue / rightValue;

            case TokenType.Star:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue * rightValue;

            case TokenType.Greater:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue > rightValue;

            case TokenType.GreaterEqual:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue >= rightValue;

            case TokenType.Less:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue < rightValue;

            case TokenType.LessEqual:
                CheckNumberOperands(expression.Op, left, right, out leftValue, out rightValue);
                return leftValue <= rightValue;

            case TokenType.BangEqual:
                return !IsEqual(left, right);

            case TokenType.EqualEqual:
                return IsEqual(left, right);
        }

        return null;
    }

    public object? Visit(UnaryExpr expression)
    {
        object? right = Evaluate(expression.Right);
        switch (expression.Op.Type)
        {
            case TokenType.Bang:
                return !IsTruthy(right);
            case TokenType.Minus:
                CheckNumberOperand(expression.Op, right, out decimal value);
                return -value;
        }

        return null;
    }

    public object? Visit(CallExpr expression)
    {
        object? callee = Evaluate(expression.Callee);

        List<object?> arguments = new List<object?>();
        foreach (var arg in expression.Arguments)
        {
            arguments.Add(Evaluate(arg));
        }

        if (callee is not null && callee is IMorphCallable function)
        {
            if (arguments.Count != function.Arity)
            {
                throw new RuntimeException(expression.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        throw new RuntimeException(expression.Paren, "Can only call functions and classes.");
    }

    public object? Visit(IndexExpr expression)
    {
        object? callee = Evaluate(expression.Callee);
        object? index = Evaluate(expression.Index);

        if (callee is not null && callee is IMorphIndexable indexable)
        {
            return indexable.Get(this, index);
        }

        throw new RuntimeException(expression.Bracket, "Can only index into indexable objects.");
    }

    public object? Visit(InterpolatedStringExpr expression)
    {
        var sb = new StringBuilder();

        foreach (var part in expression.Parts)
        {
            var stringPart = Stringify(Evaluate(part));
            sb.Append(stringPart);
        }

        return sb.ToString();
    }

    public object? Visit(LiteralExpr expression)
    {
        return expression.Value;
    }

    private object? LookUpVariable(Token name, Expr expression)
    {
        if (_locals.TryGetValue(expression, out int distance))
        {
            return _environment.GetAt(distance, name);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    private object? Evaluate(Expr expression)
    {
        return expression.Accept(this);
    }

    private bool IsTruthy(object? obj)
    {
        if (obj is null) return false;
        if (obj is bool boolean) return boolean;
        return true;
    }

    private bool IsEqual(object? left, object? right)
    {
        if (left is null && right is null) return true;
        if (left is null) return false;
        return left.Equals(right);
    }

    private void CheckNumberOperand(Token op, object? operand, out decimal value)
    {
        if (operand is decimal dec)
        {
            value = dec;
            return;
        }

        throw new RuntimeException(op, $"Operand must be a number");
    }

    private void CheckNumberOperands(Token op, object? left, object? right, out decimal leftValue, out decimal rightValue)
    {
        if (left is decimal lv && right is decimal rv)
        {
            leftValue = lv;
            rightValue = rv;
            return;
        }

        throw new RuntimeException(op, "Operands must be numbers");
    }
}