using DotnetDebug.UiTests.FlaUI.EasyUse.Pages;
using FlaUI.EasyUse.Extensions;
using FlaUI.EasyUse.Session;
using FlaUI.EasyUse.TUnit;
using TUnit.Assertions;
using TUnit.Core;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Tests.UIAutomationTests;

public sealed class MainWindowFlaUIEasyUseTests : DesktopUiTestBase<MainWindowPage>
{
    protected override DesktopProjectLaunchOptions CreateLaunchOptions()
    {
        return new DesktopProjectLaunchOptions
        {
            SolutionFileName = "DotnetDebug.sln",
            ProjectRelativePath = Path.Combine("src", "DotnetDebug.Avalonia", "DotnetDebug.Avalonia.csproj"),
            BuildConfiguration = "Debug",
            TargetFramework = "net9.0"
        };
    }

    protected override MainWindowPage CreatePage(DesktopAppSession session)
    {
        return new MainWindowPage(session.MainWindow, session.ConditionFactory);
    }

    [Test]
    [NotInParallel(DesktopUiConstraint)]
    public async Task Calculate_ValidInput_ShowsResultAndSteps()
    {
        Page
            .EnterText(p => p.NumbersInput, "48 18 30")
            .ClickButton(p => p.CalculateButton)
            .WaitUntilNameEquals(p => p.ResultText, "GCD = 6")
            .WaitUntilHasItemsAtLeast(p => p.StepsList, 1);

        using (Assert.Multiple())
        {
            await UiAssert.TextEqualsAsync(() => Page.ResultText.Text, "GCD = 6");
            await UiAssert.NumberAtLeastAsync(() => Page.StepsList.Items.Length, 9);
            await UiAssert.TextEqualsAsync(() => Page.ErrorText.Text, string.Empty);
        }
    }

    [Test]
    [NotInParallel(DesktopUiConstraint)]
    public async Task Calculate_InvalidInput_ShowsValidationError()
    {
        Page
            .EnterText(p => p.NumbersInput, "48 x 30")
            .ClickButton(p => p.CalculateButton)
            .WaitUntilNameContains(p => p.ErrorText, "Invalid integer: x");

        using (Assert.Multiple())
        {
            await UiAssert.TextContainsAsync(() => Page.ErrorText.Text, "Invalid integer: x");
            await UiAssert.TextEqualsAsync(() => Page.ResultText.Text, string.Empty);
        }
    }
}
