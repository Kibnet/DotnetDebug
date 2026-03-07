using DotnetDebug.UiTests.Authoring.Pages;
using DotnetDebug.UiTests.Authoring.Tests.UIAutomationTests;
using EasyUse.TestHost;
using EasyUse.TUnit.Core;
using FlaUI.EasyUse.Automation;
using FlaUI.EasyUse.Session;
using TUnit.Core;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Tests.UIAutomationTests;

[InheritsTests]
public sealed class MainWindowFlaUiRuntimeTests : MainWindowScenariosBase<MainWindowFlaUiRuntimeTests.FlaUiRuntimeSession>
{
    protected override FlaUiRuntimeSession LaunchSession()
    {
        return new FlaUiRuntimeSession(DesktopAppSession.Launch(DotnetDebugAppLaunchHost.CreateDesktopLaunchOptions()));
    }

    protected override MainWindowPage CreatePage(FlaUiRuntimeSession session)
    {
        return new MainWindowPage(new FlaUiControlResolver(session.Inner.MainWindow, session.Inner.ConditionFactory));
    }

    public sealed class FlaUiRuntimeSession : IUiTestSession
    {
        public FlaUiRuntimeSession(DesktopAppSession inner)
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
