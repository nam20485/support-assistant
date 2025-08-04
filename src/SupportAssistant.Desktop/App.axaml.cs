using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SupportAssistant.Desktop.ViewModels;
using SupportAssistant.Desktop.Views;
using SupportAssistant.Core.Interfaces;
using SupportAssistant.AI.Services;
using SupportAssistant.KnowledgeBase.Services;

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
            
            // Initialize services asynchronously
            _ = mainViewModel.InitializeAsync();
            
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
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .UseSerilog((context, configuration) =>
                configuration
                    .WriteTo.Console()
                    .WriteTo.File("logs/supportassistant-.txt", rollingInterval: RollingInterval.Day))
            .ConfigureServices((context, services) =>
            {
                // Register Core Services
                services.AddSingleton<IAIService, OnnxAIService>();
                services.AddSingleton<IKnowledgeBaseService, SqliteKnowledgeBaseService>();

                // Register ViewModels
                services.AddTransient<MainWindowViewModel>();
            });
    }
}