using Morph.Parsing;
using Morph.Parsing.Statements;
using Morph.Runtime;
using Morph.Scanning;

namespace Morph;

public record Message(string channel, string content);

public static class Morph
{
    public static event EventHandler<Message>? Out;

    private static Interpreter interpreter = new Interpreter();

    private static bool hadError = false;
    private static bool hadRuntimeError = false;

    public static bool RunCode(string code, Dictionary<string, string> inputs)
    {
        interpreter = new Interpreter();

        hadError = false;
        hadRuntimeError = false;

        Run(code, inputs);
        return !hadError && !hadRuntimeError;
    }

    private static void Run(string source, Dictionary<string, string> inputs)
    {
        List<Token> tokens = Scanner.ScanTokens(source);

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
        Out?.Invoke(null, new Message(channel, message));
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
}