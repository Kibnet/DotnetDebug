using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class DesktopAppUiTests
{
    [Test]
    public async Task Calculate_ValidInput_ShowsResultAndSteps()
    {
        using var session = DesktopAppSession.Start();

        session.EnterNumbers("48 18 30");
        session.ClickCalculate();

        var resultText = session.WaitForText("GCD = 6", TimeSpan.FromSeconds(10));
        var stepsCount = session.GetStepItems().Length;

        await Assert.That(resultText).IsEqualTo("GCD = 6");
        await Assert.That(stepsCount > 0).IsEqualTo(true);
    }

    [Test]
    public async Task Calculate_InvalidInput_ShowsValidationError()
    {
        using var session = DesktopAppSession.Start();

        session.EnterNumbers("48 x 30");
        session.ClickCalculate();

        var errorText = session.WaitForText("Invalid integer: x", TimeSpan.FromSeconds(10));
        await Assert.That(errorText).IsEqualTo("Invalid integer: x");
    }

    private sealed class DesktopAppSession : IDisposable
    {
        private static readonly object BuildLock = new();
        private static bool _isBuilt;

        private readonly Application _application;
        private readonly UIA3Automation _automation;
        private readonly Window _mainWindow;

        private DesktopAppSession(Application application, UIA3Automation automation, Window mainWindow)
        {
            _application = application;
            _automation = automation;
            _mainWindow = mainWindow;
        }

        public static DesktopAppSession Start()
        {
            if (!System.OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Desktop UI tests are supported only on Windows.");
            }

            var solutionRoot = FindSolutionRoot();
            EnsureDesktopAppBuilt(solutionRoot);

            var executablePath = Path.Combine(
                solutionRoot,
                "src",
                "DotnetDebug.Avalonia",
                "bin",
                "Debug",
                "net9.0",
                "DotnetDebug.Avalonia.exe");

            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException("Desktop app executable was not found.", executablePath);
            }

            var app = Application.Launch(executablePath);
            var automation = new UIA3Automation();

            var mainWindowResult = Retry.WhileNull(
                () => app.GetMainWindow(automation),
                timeout: TimeSpan.FromSeconds(10),
                interval: TimeSpan.FromMilliseconds(200),
                throwOnTimeout: false);

            if (!mainWindowResult.Success || mainWindowResult.Result is null)
            {
                automation.Dispose();
                TryClose(app);
                throw new InvalidOperationException("Desktop app main window was not found.");
            }

            return new DesktopAppSession(app, automation, mainWindowResult.Result);
        }

        public void EnterNumbers(string value)
        {
            var input = Retry.WhileNull(
                () => _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)),
                timeout: TimeSpan.FromSeconds(5),
                interval: TimeSpan.FromMilliseconds(100),
                throwOnTimeout: false).Result;

            if (input is null)
            {
                throw new InvalidOperationException("Input text box was not found.");
            }

            input.AsTextBox().Text = value;
        }

        public void ClickCalculate()
        {
            var button = Retry.WhileNull(
                () => _mainWindow.FindFirstDescendant(
                    cf => cf.ByControlType(ControlType.Button).And(cf.ByName("Calculate"))),
                timeout: TimeSpan.FromSeconds(5),
                interval: TimeSpan.FromMilliseconds(100),
                throwOnTimeout: false).Result;

            if (button is null)
            {
                throw new InvalidOperationException("Calculate button was not found.");
            }

            button.AsButton().Invoke();
        }

        public string WaitForText(string expectedText, TimeSpan timeout)
        {
            var textElementResult = Retry.WhileNull(
                () => _mainWindow.FindFirstDescendant(cf => cf.ByName(expectedText)),
                timeout: timeout,
                interval: TimeSpan.FromMilliseconds(100),
                throwOnTimeout: false);

            if (!textElementResult.Success || textElementResult.Result is null)
            {
                throw new TimeoutException($"Text '{expectedText}' was not found.");
            }

            return textElementResult.Result.Name;
        }

        public AutomationElement[] GetStepItems()
        {
            return _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem)).ToArray();
        }

        public void Dispose()
        {
            _automation.Dispose();
            TryClose(_application);
            _application.Dispose();
        }

        private static void EnsureDesktopAppBuilt(string solutionRoot)
        {
            lock (BuildLock)
            {
                if (_isBuilt)
                {
                    return;
                }

                var projectPath = Path.Combine(solutionRoot, "src", "DotnetDebug.Avalonia", "DotnetDebug.Avalonia.csproj");
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    WorkingDirectory = solutionRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                processInfo.ArgumentList.Add("build");
                processInfo.ArgumentList.Add(projectPath);
                processInfo.ArgumentList.Add("-c");
                processInfo.ArgumentList.Add("Debug");

                using var process = Process.Start(processInfo)
                    ?? throw new InvalidOperationException("Unable to start dotnet build process.");
                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Failed to build desktop app. ExitCode={process.ExitCode}{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
                }

                _isBuilt = true;
            }
        }

        private static string FindSolutionRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "DotnetDebug.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate solution root (DotnetDebug.sln).");
        }

        private static void TryClose(Application app)
        {
            try
            {
                if (!app.HasExited)
                {
                    app.Close();
                }
            }
            catch
            {
                // Ignore close errors and force kill below.
            }

            try
            {
                if (!app.HasExited)
                {
                    app.Kill();
                }
            }
            catch
            {
                // Ignore process cleanup exceptions in test teardown.
            }
        }
    }
}
