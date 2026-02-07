using System.Globalization;
using DotnetDebug;

if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    PrintUsage();
    return 0;
}

var numbers = new List<long>(args.Length);
foreach (var arg in args)
{
    if (!long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
    {
        Console.Error.WriteLine($"Invalid integer: {arg}");
        return 1;
    }

    numbers.Add(value);
}

if (numbers.Count == 0)
{
    PrintUsage();
    return 1;
}

try
{
    var gcd = GcdCalculator.ComputeGcd(numbers);
    Console.WriteLine(gcd);
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("Usage: dotnet run -- <int> <int> [int ...]");
    Console.WriteLine("Computes the greatest common divisor (GCD) of all arguments.");
}
