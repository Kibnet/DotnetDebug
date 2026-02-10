using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DotnetDebug;

namespace DotnetDebug.Avalonia;

public partial class MainWindow : Window
{
    private static readonly char[] InputSeparators = [' ', '\t', '\r', '\n', ',', ';'];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnCalculateClick(object? sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        ResultText.Text = string.Empty;
        StepsList.ItemsSource = Array.Empty<string>();

        if (!TryParseNumbers(NumbersInput.Text, out var numbers, out var errorMessage))
        {
            ErrorText.Text = errorMessage;
            return;
        }

        try
        {
            var computation = GcdCalculator.ComputeGcdWithSteps(numbers);
            ResultText.Text = $"GCD = {computation.Result}";
            StepsList.ItemsSource = BuildStepLines(computation);
        }
        catch (ArgumentException ex)
        {
            ErrorText.Text = ex.Message;
        }
    }

    private static bool TryParseNumbers(string? input, out List<long> numbers, out string errorMessage)
    {
        numbers = new List<long>();
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            errorMessage = "Provide at least one integer.";
            return false;
        }

        var tokens = input.Split(InputSeparators, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                errorMessage = $"Invalid integer: {token}";
                return false;
            }

            numbers.Add(value);
        }

        return true;
    }

    private static IReadOnlyList<string> BuildStepLines(GcdComputationResult computation)
    {
        var lines = new List<string>();

        for (var i = 0; i < computation.PairComputations.Count; i++)
        {
            var pair = computation.PairComputations[i];
            lines.Add($"Step {i + 1}: gcd({pair.Left}, {pair.Right}) = {pair.Result}");

            if (pair.Steps.Count == 0)
            {
                lines.Add("  No Euclidean divisions required.");
                continue;
            }

            foreach (var step in pair.Steps)
            {
                lines.Add($"  {step.Dividend} = {step.Divisor} * {step.Quotient} + {step.Remainder}");
            }
        }

        return lines;
    }
}
