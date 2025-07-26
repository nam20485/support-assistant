using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SupportAssistant.Desktop.ViewModels;
using SupportAssistant.Desktop.Views;

namespace SupportAssistant.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var host = CreateHostBuilder().Build();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = host.Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) => 
                configuration
                    .WriteTo.Console()
                    .WriteTo.File("logs/supportassistant-.txt", rollingInterval: RollingInterval.Day))
            .ConfigureServices((context, services) =>
            {
                // Register ViewModels
                services.AddTransient<MainWindowViewModel>();
                
                // Add other services here
            });
    }
}