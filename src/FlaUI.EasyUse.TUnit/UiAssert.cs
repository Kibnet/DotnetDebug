using System.Diagnostics;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

namespace FlaUI.EasyUse.TUnit;

public static class UiAssert
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(100);

    public static async Task TextEqualsAsync(Func<string> actualFactory, string expected, TimeSpan? timeout = null)
    {
        var actual = await WaitUntilAsync(
            actualFactory,
            value => string.Equals(value, expected, StringComparison.Ordinal),
            timeout ?? DefaultTimeout);

        await Assert.That(actual).IsEqualTo(expected);
    }

    public static async Task TextContainsAsync(Func<string> actualFactory, string expectedPart, TimeSpan? timeout = null)
    {
        var actual = await WaitUntilAsync(
            actualFactory,
            value => value.Contains(expectedPart, StringComparison.Ordinal),
            timeout ?? DefaultTimeout);

        await Assert.That(actual.Contains(expectedPart, StringComparison.Ordinal)).IsEqualTo(true);
    }

    public static async Task NumberAtLeastAsync(Func<int> actualFactory, int expectedMin, TimeSpan? timeout = null)
    {
        var actual = await WaitUntilAsync(
            actualFactory,
            value => value >= expectedMin,
            timeout ?? DefaultTimeout);

        await Assert.That(actual >= expectedMin).IsEqualTo(true);
    }

    private static async Task<T> WaitUntilAsync<T>(Func<T> actualFactory, Func<T, bool> condition, TimeSpan timeout)
    {
        if (actualFactory is null)
        {
            throw new ArgumentNullException(nameof(actualFactory));
        }

        if (condition is null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        var stopwatch = Stopwatch.StartNew();
        var lastValue = actualFactory();

        while (stopwatch.Elapsed < timeout)
        {
            if (condition(lastValue))
            {
                return lastValue;
            }

            await Task.Delay(DefaultPollInterval);
            lastValue = actualFactory();
        }

        if (!condition(lastValue))
        {
            throw new TimeoutException($"Condition was not met in {timeout.TotalMilliseconds} ms.");
        }

        return lastValue;
    }
}
