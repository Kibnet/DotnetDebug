using System.Diagnostics;
using DotnetDebug.UiTests.FlaUI.EasyUse.Controllers;
using FlaUI.EasyUse.Session;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Clients;

internal sealed class AutomationTestClient : IDisposable
{
    private static readonly object BuildLock = new();
    private static bool _desktopAppBuilt;

    private DesktopAppSession? _session;

    public MainWindowController Start(TimeSpan timeout)
    {
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

        _session = DesktopAppSession.Launch(new DesktopAppLaunchOptions
        {
            ExecutablePath = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath),
            MainWindowTimeout = timeout,
            PollInterval = TimeSpan.FromMilliseconds(200)
        });

        return new MainWindowController(_session.MainWindow, _session.ConditionFactory);
    }

    public void Kill()
    {
        Dispose();
    }

    public void Dispose()
    {
        _session?.Dispose();
        _session = null;
        GC.SuppressFinalize(this);
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

    private static void EnsureDesktopAppBuilt(string solutionRoot)
    {
        lock (BuildLock)
        {
            if (_desktopAppBuilt)
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

            _desktopAppBuilt = true;
        }
    }
}
