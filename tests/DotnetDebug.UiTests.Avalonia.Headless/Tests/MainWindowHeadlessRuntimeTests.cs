using System;
using DotnetDebug.UiTests.FlaUI.EasyUse.Pages;
using FlaUI.EasyUse.Session;
using TUnit.Core;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Tests.UIAutomationTests;

[InheritsTests]
public sealed class MainWindowHeadlessRuntimeTests : MainWindowScenariosBase
{
    protected override DesktopProjectLaunchOptions CreateLaunchOptions()
    {
        return new DesktopProjectLaunchOptions
        {
            SolutionFileName = "DotnetDebug.sln",
            ProjectRelativePath = Path.Combine("src", "DotnetDebug.Avalonia", "DotnetDebug.Avalonia.csproj"),
            BuildConfiguration = "Debug",
            TargetFramework = "net9.0"
        };
    }

    protected override MainWindowPage CreatePage(DesktopAppSession session)
    {
        return new MainWindowPage(session.MainWindow, session.ConditionFactory);
    }
}
