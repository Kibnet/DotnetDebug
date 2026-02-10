using System.Globalization;
using DotnetDebug;

const string InteractiveFlag = "-i";

if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    PrintUsage();
    return 0;
}

var numbers = new List<long>(args.Length);
var interactiveMode = args.Contains(InteractiveFlag);
var cliNumberArgs = args.Where(arg => !string.Equals(arg, InteractiveFlag, StringComparison.Ordinal));

foreach (var arg in cliNumberArgs)
{
    if (!long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
    {
        Console.Error.WriteLine($"Invalid integer: {arg}");
        return 1;
    }

    numbers.Add(value);
}

if (interactiveMode && !TryReadInteractiveNumbers(numbers))
{
    return 1;
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
    Console.WriteLine("       dotnet run -- -i [int ...]");
    Console.WriteLine("Interactive mode (-i): read one or more integers per line until empty line/EOF.");
    Console.WriteLine("Computes the greatest common divisor (GCD) of all arguments.");
}

static bool TryReadInteractiveNumbers(ICollection<long> numbers)
{
    Console.WriteLine("Interactive mode enabled. Enter integers separated by spaces.");
    Console.WriteLine("Submit an empty line (or Ctrl+Z/Ctrl+D) to finish.");

    while (true)
    {
        Console.Write("> ");
        var line = Console.ReadLine();

        if (line is null || string.IsNullOrWhiteSpace(line))
        {
            return true;
        }

        var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                Console.Error.WriteLine($"Invalid integer: {token}");
                return false;
            }

            numbers.Add(value);
        }
    }
}
