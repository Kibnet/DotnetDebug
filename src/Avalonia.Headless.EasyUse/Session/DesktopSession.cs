using AvaloniaWindow = Avalonia.Controls.Window;
using EasyUse.Session.Contracts;

namespace Avalonia.Headless.EasyUse.Session;

public sealed class DesktopAppSession : IDisposable
{
    private readonly AvaloniaWindow _nativeWindow;
    private bool _disposed;

    private DesktopAppSession(AvaloniaWindow nativeWindow)
    {
        _nativeWindow = nativeWindow;
        MainWindow = new FlaUI.Core.AutomationElements.Window(nativeWindow);
    }

    public FlaUI.Core.AutomationElements.Window MainWindow { get; }

    public static DesktopAppSession Launch(HeadlessAppLaunchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.CreateMainWindow);

        var window = HeadlessRuntime.Dispatch(() =>
        {
            var created = options.CreateMainWindow();
            return created as AvaloniaWindow
                ?? throw new InvalidOperationException("Headless launch factory must return Avalonia.Controls.Window.");
        });

        return new DesktopAppSession(window);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
