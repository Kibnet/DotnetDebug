namespace FlaUI.EasyUse.Waiting;

public sealed record class UiWaitOptions
{
    public static UiWaitOptions Default { get; } = new();

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(100);

    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;
}
