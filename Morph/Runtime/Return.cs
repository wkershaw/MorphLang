namespace Morph.Runtime;

internal class Return : Exception
{
    public object? Value { get; private set; }

    public Return(object? value) : base("Return statement executed")
    {
        Value = value;
    }
}