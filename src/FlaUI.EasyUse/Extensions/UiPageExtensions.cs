using System.Linq.Expressions;
using FlaUI.Core.AutomationElements;
using FlaUI.EasyUse.PageObjects;

namespace FlaUI.EasyUse.Extensions;

public static class UiPageExtensions
{
    public static TSelf EnterText<TSelf>(this TSelf page, Expression<Func<TSelf, TextBox>> selector, string value, int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(selector);

        var textBox = Resolve(selector, page);
        textBox.EnterText(value, timeoutMs);
        return page;
    }

    public static TSelf ClickButton<TSelf>(this TSelf page, Expression<Func<TSelf, Button>> selector, int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(selector);

        var button = Resolve(selector, page);
        button.ClickButton(timeoutMs);
        return page;
    }

    public static TSelf WaitUntilNameEquals<TSelf>(this TSelf page, Expression<Func<TSelf, AutomationElement>> selector, string expectedText, int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(selector);

        var element = Resolve(selector, page);
        if (!element.WaitUntilNameEquals(expectedText, timeoutMs))
        {
            throw new TimeoutException($"Element [{expectedText}] was not reached.");
        }

        return page;
    }

    public static TSelf WaitUntilNameContains<TSelf>(this TSelf page, Expression<Func<TSelf, AutomationElement>> selector, string expectedPart, int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(selector);

        var element = Resolve(selector, page);
        if (!element.WaitUntilNameContains(expectedPart, timeoutMs))
        {
            throw new TimeoutException($"Element did not contain '{expectedPart}'.");
        }

        return page;
    }

    public static TSelf WaitUntilHasItemsAtLeast<TSelf>(this TSelf page, Expression<Func<TSelf, ListBox>> selector, int minCount, int timeoutMs = 5000)
        where TSelf : UiPage
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(selector);

        var listBox = Resolve(selector, page);
        if (!listBox.WaitUntilHasItems(minCount, timeoutMs))
        {
            throw new TimeoutException($"ListBox did not contain at least {minCount} items.");
        }

        return page;
    }

    private static T Resolve<TSelf, T>(Expression<Func<TSelf, T>> selector, TSelf page)
        where TSelf : UiPage
    {
        var control = selector.Compile().Invoke(page);
        if (control is null)
        {
            throw new InvalidOperationException("Selector returned null.");
        }

        return control;
    }
}

