Morph.Morph.ErrorOut += (object? _, string message) =>
{
    var prev = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ForegroundColor = prev;
};

Morph.Morph.StdOut += (object? _, string message) => Console.WriteLine(message);

Morph.Morph.DebugOut += (object? _, string message) =>
{
    var prev = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(message);
    Console.ForegroundColor = prev;
};

if (args.Length == 0)
{
    Morph.Morph.RunPrompt();
}
if (args.Length == 1)
{
    Morph.Morph.RunFile(args[0]);
}
else
{
    Morph.Morph.RunFile(args[0], args.Skip(1).ToArray());
}