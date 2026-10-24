using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Simprak.App.ViewModels;
using Simprak.App.Views;
using Simprak.Core.Services;
using Simprak.Data;
using Simprak.Data.Services;

namespace Simprak.App;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        string dbPath = DatabaseInitializer.GetDefaultDatabasePath();
        DatabaseInitializer.EnsureInitialized(dbPath);
        string connString = $"Data Source={dbPath};Foreign Keys=True";

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<UserSession>();
        serviceCollection.AddSingleton(sp => new SimprakDataService(connString, sp.GetRequiredService<UserSession>()));
        serviceCollection.AddSingleton(sp => new MainWindowViewModel(
            sp.GetRequiredService<SimprakDataService>(),
            sp.GetRequiredService<UserSession>(),
            dbPath));

        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
