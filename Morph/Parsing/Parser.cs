using Morph.Parsing.Expressions;
using Morph.Parsing.Statements;
using Morph.Scanning;

namespace Morph.Parsing;

internal class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        List<Stmt> statements = [];

        while (!IsAtEnd())
        {
            Stmt? statement = Declaration();
            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        return statements;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.Class))
            {
                return ClassDeclaration();
            }

            if (Match(TokenType.Fun))
            {
                return Function("function");
            }

            if (Match(TokenType.Var))
            {
                return VarDeclaration();
            }

            return Match(TokenType.In) ? InDeclaration() : Statement();
        }
        catch (ParseException)
        {
            Synchronize();
            return null;
        }
    }

    private FunctionStmt Function(string kind)
    {
        Token name = Consume(TokenType.Identifier, $"Expect {kind} name.");

        _ = Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");
        List<Token> parameters = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Morph.Error(Peek(), "Cannot have more than 255 parameters.");
                }

                parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
            }
            while (Match(TokenType.Comma));
        }

        _ = Consume(TokenType.RightParen, $"Expect ')' after {kind} parameters.");
        _ = Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
        List<Stmt> body = BlockStatement();
        return new FunctionStmt(name, parameters, body);
    }

    private VarStmt VarDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expect variable name.");

        Expr? initialiser = null;
        if (Match(TokenType.Equal))
        {
            initialiser = Expression();
        }

        _ = Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarStmt(name, initialiser);
    }

    private ClassStmt ClassDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expect class name");
        _ = Consume(TokenType.LeftBrace, "Expect '{' before class body");

        List<FunctionStmt> methods = [];
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        _ = Consume(TokenType.RightBrace, "Expect '}' after class body");

        return new ClassStmt(name, methods);
    }

    private InStmt InDeclaration()
    {
        if (!Match(TokenType.Identifier))
        {
            throw Error(Previous(), "Expect 'in' declaration type and name.");
        }

        //Expr type = new CallExpr(new VariableExpr(Previous()), Previous(), new List<Expr>());
        Expr type = new VariableExpr(Previous());

        Token name = Consume(TokenType.Identifier, "Expect 'in' declaration name.");

        _ = Consume(TokenType.Semicolon, "Expect ';' after 'in' declaration.");
        return new InStmt(type, name);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.For))
        {
            return ForStatement();
        }

        if (Match(TokenType.If))
        {
            return IfStatement();
        }

        if (Match(TokenType.Return))
        {
            return ReturnStatement();
        }

        if (Match(TokenType.While))
        {
            return WhileStatement();
        }

        if (Match(TokenType.LeftBrace))
        {
            // Handle block statements
            return new BlockStmt(BlockStatement());
        }

        return ExpressionStatement();
    }

    private ExpressionStmt ExpressionStatement()
    {
        Expr expr = Expression();
        _ = Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new ExpressionStmt(expr);
    }

    private Stmt ForStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expect '()' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else
        {
            initializer = Match(TokenType.Var) ? VarDeclaration() : ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = Expression();
        }

        _ = Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = Expression();
        }
        _ = Consume(TokenType.RightParen, "Expect ')' after for clauses.");
        Stmt body = Statement();


        // Now just turn our for loop into a while loop lol

        if (increment != null)
        {
            body = new BlockStmt([body, new ExpressionStmt(increment)]);
        }

        condition ??= new LiteralExpr(true);

        body = new WhileStmt(condition, body);

        if (initializer != null)
        {
            body = new BlockStmt([initializer, body]);
        }

        return body;
    }

    private IfStmt IfStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        Expr condition = Expression();

        _ = Consume(TokenType.RightParen, "Expect ')' after if condition.");
        Stmt thenBranch = Statement();

        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
        {
            elseBranch = Statement();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private ReturnStmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;

        if (!Check(TokenType.Semicolon))
        {
            value = Expression();
        }

        _ = Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new ReturnStmt(keyword, value);
    }

    private WhileStmt WhileStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        Expr condition = Expression();
        _ = Consume(TokenType.RightParen, "Expect ')' after while condition.");

        Stmt body = Statement();
        return new WhileStmt(condition, body);
    }

    private List<Stmt> BlockStatement()
    {
        List<Stmt> statements = [];

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            Stmt? statement = Declaration();
            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        _ = Consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.Equal))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is VariableExpr variable)
            {
                return new AssignExpr(variable.Name, value);
            }
            else if (expr is GetExpr get)
            {
                return new SetExpr(get.Object, get.Name, value);
            }

            throw Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.Or))
        {
            Token op = Previous();
            Expr right = And();
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.And))
        {
            Token op = Previous();
            Expr right = Equality();
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(TokenType.Slash, TokenType.Star))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            Token op = Previous();
            Expr right = Unary();
            return new UnaryExpr(op, right);
        }

        return Call();
    }

    private Expr Call()
    {
        Expr expr = Index();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            if (Match(TokenType.Dot))
            {
                Token name = Consume(TokenType.Identifier, "Expect property name after '.'");
                expr = new GetExpr(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private CallExpr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];

        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Morph.Error(Peek(), "Cannot have more than 255 arguments.");
                }

                arguments.Add(Expression());
            }
            while (Match(TokenType.Comma));
        }

        Token paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");
        return new CallExpr(callee, paren, arguments);
    }

    private Expr Index()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftSquareBracket))
            {
                expr = FinishIndex(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private IndexExpr FinishIndex(Expr callee)
    {
        Expr index = Expression();
        Token bracket = Consume(TokenType.RightSquareBracket, "Expect ']' after index expression.");
        return new IndexExpr(callee, bracket, index);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False))
        {
            return new LiteralExpr(false);
        }

        if (Match(TokenType.True))
        {
            return new LiteralExpr(true);
        }

        if (Match(TokenType.Nil))
        {
            return new LiteralExpr(null);
        }

        if (Match(TokenType.Number, TokenType.String))
        {
            return new LiteralExpr(Previous().Literal);
        }

        if (Match(TokenType.This))
        {
            return new ThisExpr(Previous());
        }

        if (Match(TokenType.InterpolatedStringStart))
        {
            return InterpolatedString();
        }

        if (Match(TokenType.Identifier))
        {
            return new VariableExpr(Previous());
        }

        if (Match(TokenType.LeftParen))
        {
            Expr expr = Expression();
            _ = Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new GroupingExpr(expr);
        }

        throw Error(Peek(), $"Expect expression, but got: {Peek().Type}");
    }

    private InterpolatedStringExpr InterpolatedString()
    {
        var parts = new List<Expr>();

        while (!Check(TokenType.InterpolatedStringEnd) && !IsAtEnd())
        {
            if (Match(TokenType.String))
            {
                // String literal part
                parts.Add(new LiteralExpr(Previous().Literal));
            }
            else if (Match(TokenType.InterpolatedStringExpressionStart))
            {
                // Expression part inside [ ]
                Expr expr = Expression();
                _ = Consume(TokenType.InterpolatedStringExpressionEnd, "Expect ']' after interpolated expression.");
                parts.Add(expr);
            }
            else
            {
                Morph.Error(_current, "Unexpected token in interpolated string");
            }
        }

        // Consume InterpolatedStringEnd
        _ = Consume(TokenType.InterpolatedStringEnd, "Expect end of interpolated string.");

        return new InterpolatedStringExpr(parts);
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return Previous();
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                _ = Advance();
                return true;
            }
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        return !IsAtEnd() && Peek().Type == type;
    }

    private Token Consume(TokenType type, string message)
    {
        return Check(type) ? Advance() : throw Error(Peek(), message);
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private ParseException Error(Token token, string message)
    {
        Morph.Error(token, message);
        return new ParseException(token.Line, $"Error at line {token.Line}: {message}");
    }

    private void Synchronize()
    {
        _ = Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon)
            {
                return;
            }

            switch (Peek().Type)
            {
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Return:
                    return;
            }

            _ = Advance();
        }
    }
}