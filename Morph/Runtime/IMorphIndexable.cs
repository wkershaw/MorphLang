namespace Morph.Runtime;

internal interface IMorphIndexable
{
    object? Get(Interpreter interpreter, object? key);
}