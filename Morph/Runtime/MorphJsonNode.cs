using System.Text.Json.Nodes;
using Morph.Parsing.Statements;

namespace Morph.Runtime;

internal class MorphJsonNode : IMorphIndexable
{
    private readonly JsonNode _jsonNode;
    private readonly InStmt _statement;

    public MorphJsonNode(JsonNode jsonNode, InStmt statement)
    {
        _jsonNode = jsonNode;
        _statement = statement;
    }

    public object? Get(Interpreter interpreter, object? key)
    {
        JsonNode? value = null;

        if (key is string stringKey)
        {
            value = _jsonNode[stringKey];
        }
        else if (key is decimal intKey && intKey % 1 == 0)
        {
            value = _jsonNode[(int)intKey];
        }
        else
        {
            throw new RuntimeException(_statement.Name, "Invalid key type");
        }

        return value;
    }

    public override string ToString()
    {
        return _jsonNode.ToString();
    }
}