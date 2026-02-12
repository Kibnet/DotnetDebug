using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using DotnetDebug.Avalonia;
using TUnit.Assertions;
using TUnit.Core;

namespace DotnetDebug.UiTests.Avalonia.Headless.Tests.UIAutomationTests;

public sealed class MainWindowHeadlessTests
{
    private static readonly SemaphoreSlim UiTestGate = new(1, 1);
    private static readonly Lazy<HeadlessUnitTestSession> SharedSession = new(
        () => HeadlessUnitTestSession.StartNew(typeof(App)));

    [Test]
    public async Task Calculate_ValidInput_ShowsResultAndSteps()
    {
        await UiTestGate.WaitAsync();
        try
        {
            var state = await RunScenarioAsync("48 18 30");

            await Assert.That(state.ResultText).IsEqualTo("GCD = 6");
            await Assert.That(state.ErrorText).IsEqualTo(string.Empty);
            await Assert.That(state.StepsCount).IsGreaterThan(0);
        }
        finally
        {
            UiTestGate.Release();
        }
    }

    [Test]
    public async Task Calculate_InvalidInput_ShowsValidationError()
    {
        await UiTestGate.WaitAsync();
        try
        {
            var state = await RunScenarioAsync("48 x 30");

            await Assert.That(state.ResultText).IsEqualTo(string.Empty);
            await Assert.That(state.ErrorText).IsEqualTo("Invalid integer: x");
            await Assert.That(state.StepsCount).IsEqualTo(0);
        }
        finally
        {
            UiTestGate.Release();
        }
    }

    private static async Task<MainWindowState> RunScenarioAsync(string input)
    {
        return await SharedSession.Value.Dispatch(() =>
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
