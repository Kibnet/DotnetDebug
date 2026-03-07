using AvaloniaWindow = Avalonia.Controls.Window;
using AppAutomation.Session.Contracts;

namespace AppAutomation.Avalonia.Headless.Session;

public sealed class DesktopAppSession : IDisposable
{
    private readonly AvaloniaWindow _nativeWindow;
    private bool _disposed;

    private DesktopAppSession(AvaloniaWindow nativeWindow)
    {
        _nativeWindow = nativeWindow;
        MainWindow = nativeWindow;
    }

    public AvaloniaWindow MainWindow { get; }

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
