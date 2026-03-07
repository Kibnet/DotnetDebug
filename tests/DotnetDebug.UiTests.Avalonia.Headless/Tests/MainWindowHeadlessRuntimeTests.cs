using Avalonia.Headless.EasyUse.Session;
using DotnetDebug.UiTests.Authoring.Pages;
using DotnetDebug.UiTests.Authoring.Tests.UIAutomationTests;
using EasyUse.TestHost;
using EasyUse.TUnit.Core;
using Avalonia.Headless.EasyUse.Automation;
using TUnit.Core;

namespace DotnetDebug.UiTests.Avalonia.Headless.Tests.UIAutomationTests;

[InheritsTests]
public sealed class MainWindowHeadlessRuntimeTests : MainWindowScenariosBase<MainWindowHeadlessRuntimeTests.HeadlessRuntimeSession>
{
    protected override HeadlessRuntimeSession LaunchSession()
    {
        return new HeadlessRuntimeSession(DesktopAppSession.Launch(DotnetDebugAppLaunchHost.CreateHeadlessLaunchOptions()));
    }

    protected override MainWindowPage CreatePage(HeadlessRuntimeSession session)
    {
        return new MainWindowPage(new HeadlessControlResolver(session.Inner.MainWindow));
    }

    public sealed class HeadlessRuntimeSession : IUiTestSession
    {
        public HeadlessRuntimeSession(DesktopAppSession inner)
        {
            Inner = inner;
        }

        public DesktopAppSession Inner { get; }

        public void Dispose()
        {
            Inner.Dispose();
        }
    }
}
