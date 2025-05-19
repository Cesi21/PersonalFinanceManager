using System.Windows;
using System.Configuration;
using System.Data;
using System.IO;
using Serilog;
using System;
using Serilog.Sinks.File;

namespace PersonalFinanceManager.UI
{
    public partial class App : System.Windows.Application
    {
        public App()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalFinanceManager",
                "Logs");
            Directory.CreateDirectory(logDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                   Path.Combine(logDir, "app-.log"),
                   rollingInterval: RollingInterval.Day,
                   retainedFileCountLimit: 7)
                .CreateLogger();

            Log.Information("=== Application Starting ===");

            this.DispatcherUnhandledException += (s, e) =>
            {
                Log.Fatal(e.Exception, "Unhandled exception");
                MessageBox.Show(
                  $"Fatal error: {e.Exception.Message}",
                  "Error",
                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
                e.Handled = true;
            };

            this.Exit += (_, __) => Log.Information("=== Application Exiting ===");
        }
    }

}
