using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Exceptions;

namespace FlaUI.EasyUse.PageObjects;

public abstract class UiPage(Window window, ConditionFactory conditionFactory)
{
    protected Window Window { get; } = window ?? throw new ArgumentNullException(nameof(window));

    protected ConditionFactory ConditionFactory { get; } = conditionFactory ?? throw new ArgumentNullException(nameof(conditionFactory));

    protected AutomationElement FindElement(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locatorValue);

        var element = Window.FindFirstDescendant(CreateCondition(locatorValue, locatorKind));
        if (element is not null)
        {
            return element;
        }

        if (fallbackToName && locatorKind != UiLocatorKind.Name)
        {
            element = Window.FindFirstDescendant(CreateCondition(locatorValue, UiLocatorKind.Name));
            if (element is not null)
            {
                return element;
            }
        }

        throw new ElementNotAvailableException($"Element with locator [{locatorKind}:{locatorValue}] was not found.");
    }

    protected TextBox FindTextBox(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        return FindElement(locatorValue, locatorKind, fallbackToName).AsTextBox();
    }

    protected Button FindButton(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        return FindElement(locatorValue, locatorKind, fallbackToName).AsButton();
    }

    protected Label FindLabel(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        return FindElement(locatorValue, locatorKind, fallbackToName).AsLabel();
    }

    protected ListBox FindListBox(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        return FindElement(locatorValue, locatorKind, fallbackToName).AsListBox();
    }

    protected CheckBox FindCheckBox(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        return FindElement(locatorValue, locatorKind, fallbackToName).AsCheckBox();
    }

    protected ComboBox FindComboBox(
        string locatorValue,
        UiLocatorKind locatorKind = UiLocatorKind.AutomationId,
        bool fallbackToName = true)
    {
        return FindElement(locatorValue, locatorKind, fallbackToName).AsComboBox();
    }

    private PropertyCondition CreateCondition(string locatorValue, UiLocatorKind locatorKind)
    {
        return locatorKind switch
        {
            UiLocatorKind.AutomationId => ConditionFactory.ByAutomationId(locatorValue),
            UiLocatorKind.Name => ConditionFactory.ByName(locatorValue),
            _ => throw new ArgumentOutOfRangeException(nameof(locatorKind), locatorKind, "Unsupported locator kind.")
        };
    }
}
