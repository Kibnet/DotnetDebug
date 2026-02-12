using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using DotnetDebug.UiTests.FlaUI.EasyUse.Locators;
using FlaUI.EasyUse.Extensions;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Controllers;

public sealed class MainWindowController
{
    private readonly Window _window;
    private readonly MainWindowLocators _locators;

    public MainWindowController(Window window, ConditionFactory conditionFactory)
    {
        _window = window;
        _locators = new MainWindowLocators(window, conditionFactory);
    }

    public Window GetMainWindow()
    {
        return _window;
    }

    public MainWindowController SetNumbers(string input)
    {
        _locators.NumbersInput.EnterText(input);
        return this;
    }

    public MainWindowController ClickCalculate()
    {
        _locators.CalculateButton.ClickButton();
        return this;
    }

    public MainWindowController Pause(int milliseconds)
    {
        Thread.Sleep(milliseconds);
        return this;
    }

    public MainWindowController WaitUntilResultEquals(string expectedText, int timeoutMs = 5000)
    {
        if (!_locators.ResultText.WaitUntilNameEquals(expectedText, timeoutMs))
        {
            throw new TimeoutException($"Result text did not become '{expectedText}'.");
        }

        return this;
    }

    public MainWindowController WaitUntilErrorContains(string expectedPart, int timeoutMs = 5000)
    {
        if (!_locators.ErrorText.WaitUntilNameContains(expectedPart, timeoutMs))
        {
            throw new TimeoutException($"Error text did not contain '{expectedPart}'.");
        }

        return this;
    }

    public MainWindowController WaitUntilStepsCountAtLeast(int minCount, int timeoutMs = 5000)
    {
        if (!_locators.StepsList.WaitUntilHasItems(minCount, timeoutMs))
        {
            throw new TimeoutException($"Steps list did not reach {minCount} items.");
        }

        return this;
    }

    public string GetResultText()
    {
        return _locators.ResultText.Text;
    }

    public string GetErrorText()
    {
        return _locators.ErrorText.Text;
    }

    public int GetStepsCount()
    {
        return _locators.StepsList.Items.Length;
    }
}
