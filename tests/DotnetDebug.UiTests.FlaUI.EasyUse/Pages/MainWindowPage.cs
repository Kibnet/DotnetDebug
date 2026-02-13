using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.EasyUse.Extensions;
using FlaUI.EasyUse.PageObjects;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Pages;

[UiControl("NumbersInput", UiControlType.TextBox, "NumbersInput")]
[UiControl("CalculateButton", UiControlType.Button, "CalculateButton")]
[UiControl("ResultText", UiControlType.Label, "ResultText")]
[UiControl("ErrorText", UiControlType.Label, "ErrorText")]
[UiControl("StepsList", UiControlType.ListBox, "StepsList")]
public sealed partial class MainWindowPage(Window window, ConditionFactory conditionFactory)
    : UiPage(window, conditionFactory)
{
    public MainWindowPage SetNumbers(string input)
    {
        NumbersInput.EnterText(input);
        return this;
    }

    public MainWindowPage ClickCalculate()
    {
        CalculateButton.ClickButton();
        return this;
    }

    public MainWindowPage WaitUntilResultEquals(string expectedText, int timeoutMs = 5000)
    {
        if (!ResultText.WaitUntilNameEquals(expectedText, timeoutMs))
        {
            throw new TimeoutException($"Result text did not become '{expectedText}'.");
        }

        return this;
    }

    public MainWindowPage WaitUntilErrorContains(string expectedPart, int timeoutMs = 5000)
    {
        if (!ErrorText.WaitUntilNameContains(expectedPart, timeoutMs))
        {
            throw new TimeoutException($"Error text did not contain '{expectedPart}'.");
        }

        return this;
    }

    public MainWindowPage WaitUntilStepsCountAtLeast(int minCount, int timeoutMs = 5000)
    {
        if (!StepsList.WaitUntilHasItems(minCount, timeoutMs))
        {
            throw new TimeoutException($"Steps list did not reach {minCount} items.");
        }

        return this;
    }
}
