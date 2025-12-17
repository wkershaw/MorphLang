using Morph.Parsing.Expressions;
using Morph.Parsing.Statements;
using Morph.Scanning;

namespace Morph.Parsing;

internal class Parser
{
	private const int FUNCTION_PARAMETER_LIMIT = 255;

    public static List<Stmt> Parse(List<Token> tokens)
    {
        var tokenHelper = new ListHelper<Token>(tokens, Token.Empty);
		var statements = new List<Stmt>();

		tokenHelper.MoveNext();

        while (!tokenHelper.IsAtEnd() && tokenHelper.Current.Type != TokenType.Eof)
        {
            Stmt? statement = Declaration(tokenHelper);
            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        return statements;
    }

    private static Stmt? Declaration(ListHelper<Token> tokens)
    {
        try
        {
            if (tokens.MatchAndConsume(t => t.Type, TokenType.Class))
            {
                return ClassDeclaration(tokens);
            }

            if (tokens.MatchAndConsume(t => t.Type, TokenType.Fun))
            {
                return Function(tokens, "function");
            }

            if (tokens.MatchAndConsume(t => t.Type, TokenType.Var))
            {
                return VarDeclaration(tokens);
            }

			if (tokens.MatchAndConsume(t => t.Type, TokenType.In))
			{
				return InDeclaration(tokens);
			}

            return Statement(tokens);
        }
        catch (Exception e)
        {
			MorphRunner.Error(tokens.Current.Line, e.Message);
            Synchronize(tokens);
            return null;
        }
    }

	private static ClassStmt ClassDeclaration(ListHelper<Token> tokens)
	{
		Token name = tokens.Consume(t => t.Type == TokenType.Identifier, "Invalid class name");
		_ = tokens.Consume(t => t.Type == TokenType.LeftBrace, "Expected a '{' before class body");

		List<FunctionStmt> methods = [];
		while (!tokens.Match(t => t.Type, TokenType.RightBrace) && !tokens.IsAtEnd())
		{
			methods.Add(Function(tokens, "method"));
		}

		_ = tokens.Consume(t => t.Type == TokenType.RightBrace, "Expected a '}' after class body");

		return new ClassStmt(name, methods);
	}

	private static FunctionStmt Function(ListHelper<Token> tokens, string kind)
    {
        Token name = tokens.Consume(t => t.Type == TokenType.Identifier, $"Expected a {kind} name.");

        _ = tokens.Consume(t => t.Type == TokenType.LeftParen, $"Expected '(' after {kind} name.");
        
		List<Token> parameters = [];
		
		if (!tokens.Match(t => t.Type, TokenType.RightParen))
		{
            do
            {
                if (parameters.Count >= FUNCTION_PARAMETER_LIMIT)
                {
                    MorphRunner.Error(tokens.Peek(), $"Functions cannot have more than {FUNCTION_PARAMETER_LIMIT} parameters.");
                }

				Token parameter = tokens.Consume(t => t.Type == TokenType.Identifier, "Invalid parameter name");
                parameters.Add(parameter);
            }
            while (tokens.MatchAndConsume(t => t.Type, TokenType.Comma));
		}

        _ = tokens.Consume(t => t.Type == TokenType.RightParen, $"Expected a ')' after {kind} parameters.");
        _ = tokens.Consume(t => t.Type == TokenType.LeftBrace, $"Expected '{{' before {kind} body.");
        
		List<Stmt> body = BlockStatement(tokens);
        
		return new FunctionStmt(name, parameters, body);
    }

    private static VarStmt VarDeclaration(ListHelper<Token> tokens)
    {
        Token name = tokens.Consume(t => t.Type == TokenType.Identifier, "Invalid variable name.");

        Expr? initialiser = null;

        if (tokens.MatchAndConsume(t => t.Type, TokenType.Equal))
        {
            initialiser = Expression(tokens);
        }

        _ = tokens.Consume(t => t.Type == TokenType.Semicolon, "Expected a ';' after variable declaration.");
        return new VarStmt(name, initialiser);
    }

    private static InStmt InDeclaration(ListHelper<Token> tokens)
    {
        if (!tokens.MatchAndConsume(t => t.Type, TokenType.Identifier))
        {
			// TODO: better ex type here
            throw new InvalidOperationException("Expect 'in' declaration type and name.");
        }

		Token typeName = tokens.Consume(t => t.Type == TokenType.Identifier, "Invalid type for 'in' decleration");
        Expr type = new VariableExpr(typeName);

        Token name = tokens.Consume(t => t.Type == TokenType.Identifier, "Invalid name for 'in' declaration");

        _ = tokens.Consume(t => t.Type == TokenType.Semicolon, "Expected a ';' after 'in' declaration.");

        return new InStmt(type, name);
    }

    private static Stmt Statement(ListHelper<Token> tokens)
    {
        if (tokens.MatchAndConsume(t => t.Type, TokenType.For))
        {
            return ForStatement(tokens);
        }

        if (tokens.MatchAndConsume(t => t.Type, TokenType.If))
        {
            return IfStatement(tokens);
        }

		// Dont consume, so that the return statement can reference
		// the token
        if (tokens.Match(t => t.Type, TokenType.Return))
        {
            return ReturnStatement(tokens);
        }

        if (tokens.MatchAndConsume(t => t.Type, TokenType.While))
        {
            return WhileStatement(tokens);
        }

        if (tokens.MatchAndConsume(t => t.Type, TokenType.LeftBrace))
        {
            // Handle block statements
            return new BlockStmt(BlockStatement(tokens));
        }

        return ExpressionStatement(tokens);
    }

    private static Stmt ForStatement(ListHelper<Token> tokens)
    {
        _ = tokens.Consume(t => t.Type == TokenType.LeftParen, "Expected '(' after 'for'.");

        Stmt? initializer = null;
        if (!tokens.MatchAndConsume(t => t.Type, TokenType.Semicolon))
        {
            initializer = tokens.Match(t => t.Type, TokenType.Var) ? VarDeclaration(tokens) : ExpressionStatement(tokens);
        }
		// Semicolon will be consumed by var decleration or expression, so no need to consume it here

        Expr? condition = null;
        if (!tokens.Match(t => t.Type, TokenType.Semicolon))
        {
            condition = Expression(tokens);
        }

        _ = tokens.Consume(t => t.Type == TokenType.Semicolon, "Expected a ';' after loop condition.");

        Expr? increment = null;
        if (!tokens.Match(t => t.Type, TokenType.RightParen))
        {
            increment = Expression(tokens);
        }

        _ = tokens.Consume(t => t.Type == TokenType.RightParen, "Expected ')' at end of for clause.");
        Stmt body = Statement(tokens);

		// Lower for loop into a while loop

		// Append the increment statement to the bottom of the body
        if (increment != null)
        {
            body = new BlockStmt([body, new ExpressionStmt(increment)]);
        }

		// Wrap the body in a while loop using the providied condition
        body = new WhileStmt(condition ?? new LiteralExpr(true), body);

		// Prepend the initialiser before our new while loop
        if (initializer != null)
        {
            body = new BlockStmt([initializer, body]);
        }

        return body;
    }

    private static IfStmt IfStatement(ListHelper<Token> tokens)
    {
        _ = tokens.Consume(t => t.Type == TokenType.LeftParen, "Expected a '(' after 'if'");
        Expr condition = Expression(tokens);

        _ = tokens.Consume(t => t.Type == TokenType.RightParen, "Expected a ')' after 'if' condition");
        Stmt thenBranch = Statement(tokens);

        Stmt? elseBranch = null;
        if (tokens.MatchAndConsume(t => t.Type, TokenType.Else))
        {
            elseBranch = Statement(tokens);
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private static ReturnStmt ReturnStatement(ListHelper<Token> tokens)
    {
        Token keyword = tokens.Consume();
        Expr? value = null;

        if (!tokens.Match(t => t.Type, TokenType.Semicolon))
        {
            value = Expression(tokens);
        }

        _ = tokens.Consume(t => t.Type == TokenType.Semicolon, "Expected a ';' after return value.");
        return new ReturnStmt(keyword, value);
    }

    private static WhileStmt WhileStatement(ListHelper<Token> tokens)
    {
        _ = tokens.Consume(t => t.Type == TokenType.LeftParen, "Expected a '(' after 'while'.");
        Expr condition = Expression(tokens);

        _ = tokens.Consume(t => t.Type == TokenType.RightParen, "Expected a ')' after while condition.");

        Stmt body = Statement(tokens);
        return new WhileStmt(condition, body);
    }

    private static List<Stmt> BlockStatement(ListHelper<Token> tokens)
    {
        var statements = new List<Stmt>();

        while (!tokens.Match(t => t.Type, TokenType.RightBrace) && !tokens.IsAtEnd())
        {
            Stmt? statement = Declaration(tokens);
            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        _ = tokens.Consume(t => t.Type == TokenType.RightBrace, "Expected '}' after block.");
        return statements;
    }

	private static ExpressionStmt ExpressionStatement(ListHelper<Token> tokens)
	{
		Expr expr = Expression(tokens);
		_ = tokens.Consume(t => t.Type == TokenType.Semicolon, "Expected a ';' after expression.");
		return new ExpressionStmt(expr);
	}

	private static Expr Expression(ListHelper<Token> tokens)
    {
        return Assignment(tokens);
    }

    private static Expr Assignment(ListHelper<Token> tokens)
    {
        Expr target = Or(tokens);

        if (tokens.Match(t => t.Type, TokenType.Equal))
        {
            Token equals = tokens.Consume();
            Expr value = Assignment(tokens);

            if (target is VariableExpr variable)
            {
                return new AssignExpr(variable.Name, value);
            }
            else if (target is GetExpr get)
            {
                return new SetExpr(get.Object, get.Name, value);
            }

			// TODO: Better ex type
            throw new InvalidOperationException("Invalid assignment target.");
        }

        return target;
    }

    private static Expr Or(ListHelper<Token> tokens)
    {
        Expr expr = And(tokens);

        while (tokens.Match(t => t.Type, TokenType.Or))
        {
            Token op = tokens.Consume();
            Expr right = And(tokens);
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private static Expr And(ListHelper<Token> tokens)
    {
        Expr expr = Equality(tokens);

        while (tokens.Match(t => t.Type, TokenType.And))
        {
            Token op = tokens.Consume();
            Expr right = Equality(tokens);
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private static Expr Equality(ListHelper<Token> tokens)
    {
        Expr expr = Comparison(tokens);

        while (tokens.Match(t => t.Type, TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token op = tokens.Consume();
            Expr right = Comparison(tokens);
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private static Expr Comparison(ListHelper<Token> tokens)
    {
        Expr expr = Term(tokens);

        while (tokens.Match(t => t.Type, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token op = tokens.Consume();
            Expr right = Term(tokens);
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private static Expr Term(ListHelper<Token> tokens)
    {
        Expr expr = Factor(tokens);

        while (tokens.Match(t => t.Type, TokenType.Minus, TokenType.Plus))
        {
            Token op = tokens.Consume();
            Expr right = Factor(tokens);
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private static Expr Factor(ListHelper<Token> tokens)
    {
        Expr expr = Unary(tokens);

        while (tokens.Match(t => t.Type, TokenType.Slash, TokenType.Star))
        {
            Token op = tokens.Consume();
            Expr right = Unary(tokens);
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private static Expr Unary(ListHelper<Token> tokens)
    {
        if (tokens.Match(t => t.Type, TokenType.Bang, TokenType.Minus))
        {
            Token op = tokens.Consume();
            Expr right = Unary(tokens);
            return new UnaryExpr(op, right);
        }

        return Call(tokens);
    }

    private static Expr Call(ListHelper<Token> tokens)
    {
        Expr expr = Index(tokens);

        while (true)
        {
            if (tokens.MatchAndConsume(t => t.Type, TokenType.LeftParen))
            {
                expr = FinishCall(tokens, expr);
            }
            if (tokens.Match(t => t.Type, TokenType.Dot))
            {
                Token name = tokens.Consume(t => t.Type == TokenType.Identifier, "Expect property name after '.'");
                expr = new GetExpr(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private static CallExpr FinishCall(ListHelper<Token> tokens, Expr callee)
    {
        List<Expr> arguments = [];

        if (!tokens.Match(t => t.Type, TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= FUNCTION_PARAMETER_LIMIT)
                {
                    MorphRunner.Error(tokens.Peek(), $"Cannot have more than {FUNCTION_PARAMETER_LIMIT} arguments.");
                }

                arguments.Add(Expression(tokens));
            }
            while (tokens.MatchAndConsume(t => t.Type == TokenType.Comma));
        }

        Token paren = tokens.Consume(t => t.Type == TokenType.RightParen, "Expected ')' after arguments.");
        return new CallExpr(callee, paren, arguments);
    }

    private static Expr Index(ListHelper<Token> tokens)
    {
        Expr expr = Primary(tokens);

        while (true)
        {
            if (tokens.MatchAndConsume(t => t.Type == TokenType.LeftSquareBracket))
            {
                expr = FinishIndex(tokens, expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private static IndexExpr FinishIndex(ListHelper<Token> tokens, Expr callee)
    {
        Expr index = Expression(tokens);
        Token bracket = tokens.Consume(t => t.Type == TokenType.RightSquareBracket, "Expected ']' after index expression.");
        return new IndexExpr(callee, bracket, index);
    }

    private static Expr Primary(ListHelper<Token> tokens)
    {
        if (tokens.MatchAndConsume(t => t.Type == TokenType.False))
        {
            return new LiteralExpr(false);
        }

		if (tokens.MatchAndConsume(t => t.Type == TokenType.True))
        {
            return new LiteralExpr(true);
        }

		if (tokens.MatchAndConsume(t => t.Type == TokenType.Nil))
        {
            return new LiteralExpr(null);
        }

		if (tokens.Match(t => t.Type, TokenType.Number, TokenType.String))
        {
            return new LiteralExpr(tokens.Consume().Literal);
        }

		if (tokens.Match(t => t.Type, TokenType.This))
        {
            return new ThisExpr(tokens.Consume());
        }

		if (tokens.MatchAndConsume(t => t.Type == TokenType.InterpolatedStringStart))
        {
            return InterpolatedString(tokens);
        }

		if (tokens.Match(t => t.Type, TokenType.Identifier))
        {
            return new VariableExpr(tokens.Consume());
        }

		if (tokens.MatchAndConsume(t => t.Type == TokenType.LeftParen))
        {
            Expr expr = Expression(tokens);
            _ = tokens.Consume(t => t.Type == TokenType.RightParen, "Expected ')' after expression.");
            return new GroupingExpr(expr);
        }

		// TODO: Better ex type
        throw new InvalidOperationException($"Expected an expression, but got: {tokens.Current.Type}");
    }

    private static InterpolatedStringExpr InterpolatedString(ListHelper<Token> tokens)
    {
        var parts = new List<Expr>();

        while (!tokens.MatchAndConsume(t => t.Type, TokenType.InterpolatedStringEnd))
        {
            if (tokens.Match(t => t.Type, TokenType.String))
            {
                // String literal part
                parts.Add(new LiteralExpr(tokens.Consume().Literal));
            }
            else if (tokens.MatchAndConsume(t => t.Type, TokenType.InterpolatedStringExpressionStart))
            {
                // Expression part inside [ ]
                Expr expr = Expression(tokens);
                _ = tokens.Consume(t => t.Type == TokenType.InterpolatedStringExpressionEnd, "Expected ']' after expression in interpolated string.");
                parts.Add(expr);
            }
            else
            {
                MorphRunner.Error(tokens.Current, "Unexpected token in interpolated string");
            }
        }

        // Consume InterpolatedStringEnd
        _ = tokens.Consume(t => t.Type == TokenType.InterpolatedStringEnd, "Interpolated string was not closed");

        return new InterpolatedStringExpr(parts);
    }

	/// <summary>
	/// Attempt to find the next statement boundary to continue parsing from.
	/// </summary>
	private static void Synchronize(ListHelper<Token> tokens)
    {
        while (tokens.MoveNext())
        {
            if (tokens.Current.Type == TokenType.Semicolon)
            {
				tokens.MoveNext();
                return;
            }

            switch (tokens.Current.Type)
            {
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Return:
                    return;
            }
        }
    }
}