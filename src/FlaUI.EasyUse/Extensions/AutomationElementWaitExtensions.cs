using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;

namespace FlaUI.EasyUse.Extensions;

public static class AutomationElementWaitExtensions
{
    public static bool WaitUntilEnabled(this AutomationElement element, int timeoutMs = 5000)
    {
        return Retry.WhileFalse(
            () => element.IsEnabled,
            TimeSpan.FromMilliseconds(timeoutMs)).Success;
    }

    public static bool WaitUntilClickable(this AutomationElement element, int timeoutMs = 5000)
    {
        return Retry.WhileFalse(
            () => element.IsEnabled && !element.IsOffscreen,
            TimeSpan.FromMilliseconds(timeoutMs)).Success;
    }

    public static bool WaitUntilNameEquals(this AutomationElement element, string expectedText, int timeoutMs = 5000)
    {
        return Retry.WhileFalse(
            () => string.Equals(element.Name, expectedText, StringComparison.Ordinal),
            TimeSpan.FromMilliseconds(timeoutMs)).Success;
    }

    public static bool WaitUntilNameContains(this AutomationElement element, string expectedPart, int timeoutMs = 5000)
    {
        return Retry.WhileFalse(
            () => (element.Name ?? string.Empty).Contains(expectedPart, StringComparison.Ordinal),
            TimeSpan.FromMilliseconds(timeoutMs)).Success;
    }

    public static bool WaitUntilHasItems(this ListBox listBox, int minCount, int timeoutMs = 5000)
    {
        return Retry.WhileFalse(
            () => listBox.Items.Length >= minCount,
            TimeSpan.FromMilliseconds(timeoutMs)).Success;
    }
}
