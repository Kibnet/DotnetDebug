using DotnetDebug.UiTests.UIAutomationTestKit.Clients;
using FlaUI.EasyUse.TUnit;
using TUnit.Core;

namespace DotnetDebug.UiTests.UIAutomationTestKit.Tests.UIAutomationTests;

public sealed class MainWindowUiAutomationTestKitTests
{
    private static readonly SemaphoreSlim UiTestGate = new(1, 1);

    [Test]
    public async Task Calculate_ValidInput_ShowsResultAndSteps()
    {
        await UiTestGate.WaitAsync();
        try
        {
            using var testClient = new AutomationTestClient();
            var mainWindow = testClient.Start(TimeSpan.FromSeconds(20));

            mainWindow
                .SetNumbers("48 18 30")
                .ClickCalculate()
                .WaitUntilResultEquals("GCD = 6")
                .WaitUntilStepsCountAtLeast(1);

            await UiAssert.TextEqualsAsync(mainWindow.GetResultText, "GCD = 6");
            await UiAssert.NumberAtLeastAsync(mainWindow.GetStepsCount, 1);
            await UiAssert.TextEqualsAsync(mainWindow.GetErrorText, string.Empty);
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
            using var testClient = new AutomationTestClient();
            var mainWindow = testClient.Start(TimeSpan.FromSeconds(20));

            mainWindow
                .SetNumbers("48 x 30")
                .ClickCalculate()
                .WaitUntilErrorContains("Invalid integer: x");

            await UiAssert.TextContainsAsync(mainWindow.GetErrorText, "Invalid integer: x");
            await UiAssert.TextEqualsAsync(mainWindow.GetResultText, string.Empty);
        }
        finally
        {
            UiTestGate.Release();
        }
    }
}
