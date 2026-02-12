using DotnetDebug.UiTests.FlaUI.EasyUse.Clients;
using DotnetDebug.UiTests.FlaUI.EasyUse.Controllers;
using TUnit.Assertions;
using TUnit.Core;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Tests.UIAutomationTests;

public sealed class MainWindowFlaUIEasyUseTests
{
    private const string DesktopUiConstraint = "DesktopUi";
    private AutomationTestClient? _testClient;
    private MainWindowController? _mainWindow;

    [Before(Test)]
    public void Setup()
    {
        _testClient = new AutomationTestClient();
        _mainWindow = _testClient.Start(TimeSpan.FromSeconds(20));
    }

    [After(Test)]
    public void Cleanup()
    {
        _testClient?.Dispose();
        _testClient = null;
        _mainWindow = null;
    }

    [Test]
    [NotInParallel(DesktopUiConstraint)]
    public async Task Calculate_ValidInput_ShowsResultAndSteps()
    {
        MainWindow
            .SetNumbers("48 18 30")
            .ClickCalculate()
            .WaitUntilResultEquals("GCD = 6")
            .WaitUntilStepsCountAtLeast(1);

        using (Assert.Multiple())
        {
            await Assert.That(MainWindow.GetResultText()).IsEqualTo("GCD = 6");
            await Assert.That(MainWindow.GetStepsCount()).IsGreaterThanOrEqualTo(1);
            await Assert.That(MainWindow.GetErrorText()).IsEqualTo(string.Empty);
        }
    }

    [Test]
    [NotInParallel(DesktopUiConstraint)]
    public async Task Calculate_InvalidInput_ShowsValidationError()
    {
        MainWindow
            .SetNumbers("48 x 30")
            .ClickCalculate()
            .WaitUntilErrorContains("Invalid integer: x");

        using (Assert.Multiple())
        {
            await Assert.That(MainWindow.GetErrorText()).Contains("Invalid integer: x");
            await Assert.That(MainWindow.GetResultText()).IsEqualTo(string.Empty);
        }
    }

    private MainWindowController MainWindow =>
        _mainWindow ?? throw new InvalidOperationException("Main window was not initialized.");
}
