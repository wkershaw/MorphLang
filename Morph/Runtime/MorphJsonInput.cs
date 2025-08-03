using System.Text.Json.Nodes;
using Morph.Parsing.Statements;

namespace Morph.Runtime;

internal class MorphJsonInput : IMorphIndexable
{
    private readonly InStmt _statement;
    private readonly JsonObject json;

    public MorphJsonInput(Interpreter interpreter, InStmt statement)
    {
        _statement = statement;

        var obj = interpreter.ParsedInputs.ContainsKey(_statement.Name.Lexeme)
            ? interpreter.ParsedInputs[_statement.Name.Lexeme]
            : throw new RuntimeException(_statement.Name, "Input not found: " + _statement.Name.Lexeme);

        json = obj as JsonObject
            ?? throw new RuntimeException(_statement.Name, "Valid input not found: " + _statement.Name.Lexeme);

    }

    public object? Get(Interpreter interpreter, object? key)
    {
        JsonNode? value = key switch
        {
            string stringKey => json[stringKey],
            decimal intKey when intKey % 1 == 0 => json[(int)intKey],
            _ => throw new RuntimeException(_statement.Name, "Invalid key type")
        };

        if (value is null)
        {
            return null;
        }

        return new MorphJsonNode(value, _statement);
    }

    public override string ToString()
    {
        return json.ToString();
    }
}