namespace FlaUI.EasyUse.Session;

public sealed class DesktopAppLaunchOptions
{
    public string ExecutablePath { get; init; } = string.Empty;

    public string? WorkingDirectory { get; init; }

    public TimeSpan MainWindowTimeout { get; init; } = TimeSpan.FromSeconds(20);

    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(200);
}
