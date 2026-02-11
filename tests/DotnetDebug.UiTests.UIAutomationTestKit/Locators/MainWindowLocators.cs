using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Exceptions;

namespace DotnetDebug.UiTests.UIAutomationTestKit.Locators;

internal sealed class MainWindowLocators
{
    private readonly Window _window;
    private readonly ConditionFactory _conditionFactory;

    public MainWindowLocators(Window window, ConditionFactory conditionFactory)
    {
        _window = window;
        _conditionFactory = conditionFactory;
    }

    public TextBox NumbersInput => FindByAutomationId("NumbersInput").AsTextBox();

    public Button CalculateButton => FindByAutomationId("CalculateButton").AsButton();

    public Label ResultText => FindByAutomationId("ResultText").AsLabel();

    public Label ErrorText => FindByAutomationId("ErrorText").AsLabel();

    public ListBox StepsList => FindByAutomationId("StepsList").AsListBox();

    private AutomationElement FindByAutomationId(string automationId)
    {
        var byAutomationId = _window.FindFirstDescendant(_conditionFactory.ByAutomationId(automationId));
        if (byAutomationId is not null)
        {
            return byAutomationId;
        }

        var byName = _window.FindFirstDescendant(_conditionFactory.ByName(automationId));
        if (byName is not null)
        {
            return byName;
        }

        throw new ElementNotAvailableException($"Element with AutomationId [{automationId}] not found.");
    }
}
