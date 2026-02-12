using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using DotnetDebug.Avalonia;
using DotnetDebug.UiTests.Avalonia.Headless.Infrastructure;
using TUnit.Assertions;
using TUnit.Core;

namespace DotnetDebug.UiTests.Avalonia.Headless.Tests.UIAutomationTests;

public sealed class MainWindowHeadlessTests
{
    private const string HeadlessUiConstraint = "AvaloniaHeadlessUi";

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Calculate_ValidInput_ShowsResultAndSteps()
    {
        var state = await RunScenarioAsync("48 18 30");

        using (Assert.Multiple())
        {
            await Assert.That(state.ResultText).IsEqualTo("GCD = 6");
            await Assert.That(state.ErrorText).IsEqualTo(string.Empty);
            await Assert.That(state.StepsCount).IsGreaterThan(0);
        }
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Calculate_InvalidInput_ShowsValidationError()
    {
        var state = await RunScenarioAsync("48 x 30");

        using (Assert.Multiple())
        {
            await Assert.That(state.ResultText).IsEqualTo(string.Empty);
            await Assert.That(state.ErrorText).IsEqualTo("Invalid integer: x");
            await Assert.That(state.StepsCount).IsEqualTo(0);
        }
    }

    private static async Task<MainWindowState> RunScenarioAsync(string input)
    {
        return await HeadlessSessionHooks.Session.Dispatch(() =>
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            try
            {
                var numbersInput = mainWindow.FindControl<TextBox>("NumbersInput")
                    ?? throw new InvalidOperationException("NumbersInput control was not found.");
                var calculateButton = mainWindow.FindControl<Button>("CalculateButton")
                    ?? throw new InvalidOperationException("CalculateButton control was not found.");
                var resultText = mainWindow.FindControl<TextBlock>("ResultText")
                    ?? throw new InvalidOperationException("ResultText control was not found.");
                var errorText = mainWindow.FindControl<TextBlock>("ErrorText")
                    ?? throw new InvalidOperationException("ErrorText control was not found.");
                var stepsList = mainWindow.FindControl<ListBox>("StepsList")
                    ?? throw new InvalidOperationException("StepsList control was not found.");

                numbersInput.Text = input;
                calculateButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                return new MainWindowState(
                    resultText.Text ?? string.Empty,
                    errorText.Text ?? string.Empty,
                    stepsList.ItemCount);
            }
            finally
            {
                mainWindow.Close();
            }
        }, CancellationToken.None);
    }

    private sealed record MainWindowState(string ResultText, string ErrorText, int StepsCount);
}
