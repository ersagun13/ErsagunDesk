using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace ErsagunDesk;

public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 1. Single Instance Protection via Named Mutex
        const string mutexName = "Local\\ErsagunDesk_SingleInstance_Unique_Mutex";
        _singleInstanceMutex = new Mutex(true, mutexName, out bool isNewInstance);
        if (!isNewInstance)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        // 2. Global Unhandled Exception Handlers writing to %APPDATA%\ErsagunDesk\crash.log
        DispatcherUnhandledException += (s, args) =>
        {
            LogCrash("DispatcherUnhandledException:\n" + args.Exception);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            LogCrash("AppDomain.UnhandledException:\n" + (args.ExceptionObject?.ToString() ?? "Unknown exception"));
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_singleInstanceMutex != null)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
            }
            catch { }
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }
        base.OnExit(e);
    }

    private static void LogCrash(string message)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "ErsagunDesk");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var logPath = Path.Combine(dir, "crash.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n\n");
        }
        catch { }
    }
}
