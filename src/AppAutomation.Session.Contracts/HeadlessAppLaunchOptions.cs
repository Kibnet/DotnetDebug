namespace AppAutomation.Session.Contracts;

public sealed class HeadlessAppLaunchOptions
{
    public required Func<object> CreateMainWindow { get; init; }
}
