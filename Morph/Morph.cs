using Morph.Parsing;
using Morph.Parsing.Statements;
using Morph.Parsing.Visitors;
using Morph.Runtime;
using Morph.Scanning;

namespace Morph;

public static class Morph
{
    public static event EventHandler<string>? StdOut;
    public static event EventHandler<string>? ErrorOut;
    public static event EventHandler<string>? DebugOut;

    private static Interpreter interpreter = new Interpreter();

    private static bool hadError = false;
    private static bool hadRuntimeError = false;

    public static void RunFile(string path, params string[] inputFilePaths)
    {
        string code = File.ReadAllText(path);
		Dictionary<string, string> inputs = GenerateInputDict(inputFilePaths);

		Run(code, inputs);

        if (hadError)
        {
            System.Environment.Exit(65);
        }

        if (hadRuntimeError)
        {
            System.Environment.Exit(70);
        }
    }

	public static bool RunCode(string code, Dictionary<string, string> inputs)
	{
		interpreter = new Interpreter();

        hadError = false;
        hadRuntimeError = false;

		Run(code, inputs);
        return !hadError && !hadRuntimeError;
	}

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("Morph> ");
            string? line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
                break;

            Run(line, []);
            hadError = false;
            hadRuntimeError = false;
        }
    }

    private static void Run(string source, Dictionary<string, string> inputs)
    {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        var parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();

        if (hadError)
        {
            return;
        }

        Resolver resolver = new Resolver(interpreter);
        resolver.Resolve(statements);

        if (hadError)
        {
            return;
        }

        interpreter.Interpret(statements, inputs);
    }

    internal static void Output(string channel, string message)
    {
        switch (channel)
        {
            case "error":
                ErrorOut?.Invoke(null, message);
                return;
            case "debug":
                DebugOut?.Invoke(null, message);
                return;
        }

        StdOut?.Invoke(null, message);
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Report(token.Line, "at end", message);
        }
        else
        {
            Report(token.Line, $"at '{token.Lexeme}'", message);
        }
    }

    internal static void RuntimeError(RuntimeException error)
    {
        Output("error", $"[line {error.Token?.Line}] Runtime Error: {error.Message}");
        hadRuntimeError = true;
    }

    internal static void Report(int line, string where, string message)
    {
        Output("error", $"[line {line}] Error {where}: {message}");
        hadError = true;
    }

    private static Dictionary<string, string> GenerateInputDict(string[] filePaths)
    {
        var dict = new Dictionary<string, string>();

        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileContent = File.ReadAllText(filePath);

            dict.Add(fileName, fileContent);
        }

        return dict;
    }
}