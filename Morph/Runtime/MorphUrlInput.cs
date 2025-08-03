using System.Web;
using Morph.Parsing.Statements;

namespace Morph.Runtime;

internal class MorphUrlInput : IMorphIndexable
{
    private readonly InStmt _statement;
    private readonly string _url;

    public MorphUrlInput(Interpreter interpreter, InStmt statement)
    {
        _statement = statement;

        var url = interpreter.ParsedInputs.ContainsKey(_statement.Name.Lexeme)
        ? interpreter.ParsedInputs[_statement.Name.Lexeme]
        : throw new RuntimeException(_statement.Name, "Input not found" + _statement.Name.Lexeme);

        if (url is not string urlString || urlString is null)
        {
            throw new RuntimeException(_statement.Name, "Valid input not found" + _statement.Name.Lexeme);
        }

        _url = urlString;
    }

    public object? Get(Interpreter interpreter, object? key)
    {
        if (key is null || key is not string keyString)
        {
            throw new RuntimeException(_statement.Name, "Invalid key type");
        }

        var queryString = HttpUtility.ParseQueryString(_url);
        return queryString[keyString];
    }

    public override string ToString()
    {
        return _url;
    }
}