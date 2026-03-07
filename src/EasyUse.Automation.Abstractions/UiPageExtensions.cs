using System.Globalization;
using System.Linq.Expressions;

namespace EasyUse.Automation.Abstractions;

public static class UiPageExtensions
{
    public static TSelf EnterText<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ITextBoxControl>> selector,
        string value,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var textBox = Resolve(selector, page);
        textBox.Enter(value);
        WaitUntil(
            () => string.Equals(textBox.Text, value, StringComparison.Ordinal),
            timeoutMs,
            $"TextBox '{textBox.AutomationId}' did not reach '{value}'.");
        return page;
    }

    public static TSelf ClickButton<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IButtonControl>> selector,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var button = Resolve(selector, page);
        WaitUntil(() => button.IsEnabled, timeoutMs, $"Button '{button.AutomationId}' is not enabled.");
        button.Invoke();
        return page;
    }

    public static TSelf SetChecked<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ICheckBoxControl>> selector,
        bool isChecked,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var checkBox = Resolve(selector, page);
        WaitUntil(() => checkBox.IsEnabled, timeoutMs, $"CheckBox '{checkBox.AutomationId}' is not enabled.");
        checkBox.IsChecked = isChecked;
        WaitUntil(() => checkBox.IsChecked == isChecked, timeoutMs, $"CheckBox '{checkBox.AutomationId}' did not become '{isChecked}'.");
        return page;
    }

    public static TSelf SetChecked<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IRadioButtonControl>> selector,
        bool isChecked,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var radioButton = Resolve(selector, page);
        WaitUntil(() => radioButton.IsEnabled, timeoutMs, $"RadioButton '{radioButton.AutomationId}' is not enabled.");
        radioButton.IsChecked = isChecked;
        WaitUntil(() => radioButton.IsChecked == isChecked, timeoutMs, $"RadioButton '{radioButton.AutomationId}' did not become '{isChecked}'.");
        return page;
    }

    public static TSelf SetToggled<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IToggleButtonControl>> selector,
        bool isToggled,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var toggle = Resolve(selector, page);
        WaitUntil(() => toggle.IsEnabled, timeoutMs, $"Toggle '{toggle.AutomationId}' is not enabled.");
        if (toggle.IsToggled != isToggled)
        {
            toggle.Toggle();
        }

        WaitUntil(() => toggle.IsToggled == isToggled, timeoutMs, $"Toggle '{toggle.AutomationId}' did not become '{isToggled}'.");
        return page;
    }

    public static TSelf SelectComboItem<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IComboBoxControl>> selector,
        string itemText,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemText);

        var comboBox = Resolve(selector, page);
        WaitUntil(() => comboBox.IsEnabled, timeoutMs, $"ComboBox '{comboBox.AutomationId}' is not enabled.");
        comboBox.Expand();

        var target = NormalizeLookupText(itemText);
        var index = comboBox.Items
            .Select((item, candidateIndex) => (Item: item, Index: candidateIndex))
            .FirstOrDefault(candidate =>
                string.Equals(NormalizeLookupText(candidate.Item.Text), target, StringComparison.OrdinalIgnoreCase)
                || string.Equals(NormalizeLookupText(candidate.Item.Name), target, StringComparison.OrdinalIgnoreCase))
            .Index;

        if (index < 0 || index >= comboBox.Items.Count)
        {
            throw new InvalidOperationException($"ComboBox item '{itemText}' was not found.");
        }

        comboBox.Select(index);
        WaitUntil(
            () => comboBox.SelectedIndex == index
                || string.Equals(NormalizeLookupText(comboBox.SelectedItem?.Text), target, StringComparison.OrdinalIgnoreCase)
                || string.Equals(NormalizeLookupText(comboBox.SelectedItem?.Name), target, StringComparison.OrdinalIgnoreCase),
            timeoutMs,
            $"ComboBox '{comboBox.AutomationId}' did not select '{itemText}'.");
        return page;
    }

    public static TSelf SetSliderValue<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ISliderControl>> selector,
        double value,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var slider = Resolve(selector, page);
        WaitUntil(() => slider.IsEnabled, timeoutMs, $"Slider '{slider.AutomationId}' is not enabled.");
        slider.Value = value;
        WaitUntil(() => Math.Abs(slider.Value - value) < 0.001, timeoutMs, $"Slider '{slider.AutomationId}' did not reach '{value}'.");
        return page;
    }

    public static TSelf SetSpinnerValue<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ITextBoxControl>> selector,
        double value,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var textBox = Resolve(selector, page);
        var expected = value.ToString(CultureInfo.InvariantCulture);
        textBox.Enter(expected);
        WaitUntil(
            () => string.Equals(textBox.Text?.Trim(), expected, StringComparison.Ordinal),
            timeoutMs,
            $"Spinner-like text box '{textBox.AutomationId}' did not reach '{expected}'.");
        return page;
    }

    public static TSelf SelectTabItem<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ITabControl>> selector,
        string itemText,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemText);

        var tab = Resolve(selector, page);
        WaitUntil(() => tab.IsEnabled, timeoutMs, $"Tab '{tab.AutomationId}' is not enabled.");
        tab.SelectTabItem(itemText);

        WaitUntil(
            () => tab.Items.Any(item =>
                item.IsSelected &&
                TextMatches(item.Name, itemText)),
            timeoutMs,
            $"Tab '{tab.AutomationId}' did not select '{itemText}'.");
        return page;
    }

    public static TSelf SelectTreeItem<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ITreeControl>> selector,
        string itemText,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemText);

        var tree = Resolve(selector, page);
        WaitUntil(() => tree.IsEnabled, timeoutMs, $"Tree '{tree.AutomationId}' is not enabled.");

        var target = FindTreeItem(tree.Items, itemText);
        if (target is null)
        {
            throw new InvalidOperationException($"Tree item '{itemText}' was not found.");
        }

        target.Select();
        WaitUntil(
            () => target.IsSelected
                || TextMatches(tree.SelectedTreeItem?.Text, itemText)
                || TextMatches(tree.SelectedTreeItem?.Name, itemText),
            timeoutMs,
            $"Tree item '{itemText}' was not selected.");
        return page;
    }

    public static TSelf SetDate<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IDateTimePickerControl>> selector,
        DateTime date,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var datePicker = Resolve(selector, page);
        WaitUntil(() => datePicker.IsEnabled, timeoutMs, $"Date picker '{datePicker.AutomationId}' is not enabled.");
        datePicker.SelectedDate = date.Date;
        WaitUntil(() => datePicker.SelectedDate?.Date == date.Date, timeoutMs, $"Date picker '{datePicker.AutomationId}' did not reach '{date:yyyy-MM-dd}'.");
        return page;
    }

    public static TSelf SetDate<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, ICalendarControl>> selector,
        DateTime date,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var calendar = Resolve(selector, page);
        WaitUntil(() => calendar.IsEnabled, timeoutMs, $"Calendar '{calendar.AutomationId}' is not enabled.");
        calendar.SelectDate(date.Date);
        WaitUntil(() => calendar.SelectedDates.Any(candidate => candidate.Date == date.Date), timeoutMs, $"Calendar '{calendar.AutomationId}' did not reach '{date:yyyy-MM-dd}'.");
        return page;
    }

    public static TSelf WaitUntilProgressAtLeast<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IProgressBarControl>> selector,
        double expectedMin,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var progressBar = Resolve(selector, page);
        WaitUntil(() => progressBar.Value >= expectedMin, timeoutMs, $"Progress bar '{progressBar.AutomationId}' did not reach '{expectedMin}'.");
        return page;
    }

    public static TSelf WaitUntilIsSelected<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IRadioButtonControl>> selector,
        bool expected,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var radioButton = Resolve(selector, page);
        WaitUntil(() => radioButton.IsChecked == expected, timeoutMs, $"Radio button '{radioButton.AutomationId}' did not become '{expected}'.");
        return page;
    }

    public static TSelf WaitUntilIsToggled<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IToggleButtonControl>> selector,
        bool expected,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var toggle = Resolve(selector, page);
        WaitUntil(() => toggle.IsToggled == expected, timeoutMs, $"Toggle '{toggle.AutomationId}' did not become '{expected}'.");
        return page;
    }

    public static TSelf WaitUntilListBoxContains<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IListBoxControl>> selector,
        string expectedText,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var listBox = Resolve(selector, page);
        WaitUntil(
            () => listBox.Items.Any(item =>
                (item.Text ?? item.Name ?? string.Empty).Contains(expectedText, StringComparison.Ordinal)),
            timeoutMs,
            $"ListBox '{listBox.AutomationId}' did not contain '{expectedText}'.");
        return page;
    }

    public static TSelf WaitUntilNameEquals<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IUiControl>> selector,
        string expectedText,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var control = Resolve(selector, page);
        WaitUntil(
            () => string.Equals(control.Name, expectedText, StringComparison.Ordinal),
            timeoutMs,
            $"Control '{control.AutomationId}' did not reach '{expectedText}'.");
        return page;
    }

    public static TSelf WaitUntilNameContains<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IUiControl>> selector,
        string expectedPart,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var control = Resolve(selector, page);
        WaitUntil(
            () => control.Name.Contains(expectedPart, StringComparison.Ordinal),
            timeoutMs,
            $"Control '{control.AutomationId}' did not contain '{expectedPart}'.");
        return page;
    }

    public static TSelf WaitUntilHasItemsAtLeast<TSelf>(
        this TSelf page,
        Expression<Func<TSelf, IListBoxControl>> selector,
        int minCount,
        int timeoutMs = 5000)
        where TSelf : UiPage
    {
        var listBox = Resolve(selector, page);
        WaitUntil(
            () => listBox.Items.Count >= minCount,
            timeoutMs,
            $"ListBox '{listBox.AutomationId}' did not reach '{minCount}' items.");
        return page;
    }

    private static TControl Resolve<TSelf, TControl>(Expression<Func<TSelf, TControl>> selector, TSelf page)
        where TSelf : UiPage
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(page);

        var control = selector.Compile().Invoke(page);
        if (control is null)
        {
            throw new InvalidOperationException("Selector returned null.");
        }

        return control;
    }

    private static void WaitUntil(Func<bool> condition, int timeoutMs, string timeoutMessage)
    {
        UiWait.Until(
            condition,
            static value => value,
            new UiWaitOptions
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs),
                PollInterval = TimeSpan.FromMilliseconds(100)
            },
            timeoutMessage);
    }

    private static ITreeItemControl? FindTreeItem(IEnumerable<ITreeItemControl> items, string itemText)
    {
        foreach (var item in items)
        {
            if (TextMatches(item.Text, itemText) || TextMatches(item.Name, itemText))
            {
                return item;
            }

            try
            {
                item.Expand();
            }
            catch
            {
                // Tree expansion is best effort for mixed runtimes.
            }

            var nested = FindTreeItem(item.Items, itemText);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static bool TextMatches(string? actual, string expected)
    {
        return string.Equals(NormalizeLookupText(actual), NormalizeLookupText(expected), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeLookupText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}
