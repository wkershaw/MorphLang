using System.Text;

List<FileData> GenerateClasses(string expressions, ClassType type)
{
    var files = new List<FileData>();

    var inheritsFrom = type switch
    {
        ClassType.Expression => "Expr",
        ClassType.Statement => "Stmt",
        _ => throw new Exception("Invalid class type: " + type)
    };

    var visitorInterface = type switch
    {
        ClassType.Expression => "IExpressionVisitor<T>",
        ClassType.Statement => "IStmtVisitor<T>",
        _ => throw new Exception("Invalid class type: " + type)
    };

    var ns = type switch
    {
        ClassType.Expression => "Morph.Parsing.Expressions",
        ClassType.Statement => "Morph.Parsing.Statements",
        _ => throw new Exception("Invalid class type: " + type)
    };

    var expressionList = expressions
        .Split('\n')
        .Select(line => line.Trim());

    foreach (var expression in expressionList)
    {
        var sb = new StringBuilder();

        var parts = expression.Split(':');
        if (parts.Length != 2) throw new Exception("Invalid expression format: " + expression);

        var className = parts[0].Trim();
        var properties = parts[1].Trim().Split(',')
            .Select(p => p.Trim())
            .ToArray();

        var propertyCode = properties
            .Select(p => $"    public {p} {{ get; private set; }}");

        var constructorParams = properties
            .Select(p => p.Split(' ')[0] + " " + p.Split(' ')[1].ToLower())
            .ToArray();

        var initCode = properties
            .Select(p => p.Split(' ')[1])
            .Select(name => $"        {name} = {name.ToLower()};")
            .ToArray();

        sb.Append($$"""
            using Morph.Scanning;
            using Morph.Parsing.Visitors;

            namespace {{ns}};

            internal class {{className}} : {{inheritsFrom}}
            {
            {{string.Join("\n", propertyCode)}}

                public {{className}}({{string.Join(", ", constructorParams)}})
                {
            {{string.Join("\n", initCode)}}
                }

                public override T Accept<T>({{visitorInterface}} visitor)
                {
                    return visitor.Visit(this);
                }
            }   
            """);

        files.Add(new FileData($"{className}.cs", sb.ToString()));
    }

    return files;
}

string expressions = """
    AssignExpr: Token Name, Expr? Value
    BinaryExpr: Expr Left, Token Op, Expr Right
    CallExpr: Expr Callee, Token Paren, List<Expr> Arguments
    GroupingExpr: Expr Expression
    IndexExpr: Expr Callee, Token Bracket, Expr Index
    InterpolatedStringExpr: List<Expr> Parts
    LiteralExpr: object? Value
    LogicalExpr: Expr Left, Token Op, Expr Right
    UnaryExpr: Token Op, Expr Right
    VariableExpr: Token Name
    """;


var files = GenerateClasses(expressions, ClassType.Expression);
foreach (var file in files)
{
    File.WriteAllText($"bin\\out\\Expressions\\{file.fileName}", file.content);
}

string statements = """
    BlockStmt: List<Stmt> Statements
    ExpressionStmt: Expr Expression
    FunctionStmt: Token Name, List<Token> Parameters, List<Stmt> Body
    IfStmt: Expr Condition, Stmt ThenBranch, Stmt? ElseBranch
    InStmt: Token Type, Token Name
    ReturnStmt: Token Keyword, Expr? Value
    VarStmt: Token Name, Expr? Initializer
    WhileStmt: Expr Condition, Stmt Body
    """;


files = GenerateClasses(statements, ClassType.Statement);
foreach (var file in files)
{
    File.WriteAllText($"bin\\out\\Statements\\{file.fileName}", file.content);
}

enum ClassType
{
    Expression,
    Statement,
}

record FileData(string fileName, string content);