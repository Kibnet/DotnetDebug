using AppAutomation.Session.Contracts;
using DotnetDebug.AppAutomation.TestHost;
using TUnit.Assertions;
using TUnit.Core;

public class LaunchOptionsDefaultsTests
{
    [Test]
    public async Task DotnetDebugHeadlessLaunchOptions_ExposeMainWindowFactory()
    {
        var options = DotnetDebugAppLaunchHost.CreateHeadlessLaunchOptions();

        await Assert.That(options.CreateMainWindow is not null).IsEqualTo(true);
    }

    [Test]
    public async Task DesktopAppLaunchOptions_DefaultsRemainStable()
    {
        var options = new DesktopAppLaunchOptions
        {
            ExecutablePath = "DotnetDebug.Avalonia.exe"
        };

        using (Assert.Multiple())
        {
            await Assert.That(options.WorkingDirectory).IsNull();
            await Assert.That(options.MainWindowTimeout).IsEqualTo(TimeSpan.FromSeconds(20));
            await Assert.That(options.PollInterval).IsEqualTo(TimeSpan.FromMilliseconds(200));
        }
    }
}
