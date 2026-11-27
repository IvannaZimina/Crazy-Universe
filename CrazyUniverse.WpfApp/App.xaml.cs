using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Storage;
using CrazyUniverse.Core.World;
using CrazyUniverse.WpfApp.Services;
using CrazyUniverse.WpfApp.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Windows;

namespace CrazyUniverse.WpfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Holds the root DI container for the application lifetime
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Load storage provider and connection string from appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        string provider = configuration["Storage:Provider"]?.Trim() ?? "Json";
        string connectionString = configuration["Storage:ConnectionString"]?.Trim() ?? string.Empty;

        var services = new ServiceCollection();

        // Register world, validation, mapping, and dialog services
        services.AddSingleton<ZooWorld>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<WorldMapper>();
        services.AddSingleton<IFileDialogService, FileDialogService>();

        // Register the active storage implementation according to Storage:Provider
        services.AddSingleton<IWorldStorage>(_ => CreateStorage(provider, connectionString));

        // Register MainViewModel and MainWindow using constructor injection
        services.AddTransient(sp => new MainViewModel(
            sp.GetRequiredService<ZooWorld>(),
            sp.GetRequiredService<IWorldStorage>(),
            sp.GetRequiredService<WorldMapper>(),
            sp.GetRequiredService<IFileDialogService>(),
            provider,
            connectionString));

        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose the DI container cleanly when the application exits
        _serviceProvider?.Dispose();
        _serviceProvider = null;
        base.OnExit(e);
    }

    private static IWorldStorage CreateStorage(string provider, string connectionString)
    {
        return provider.ToLowerInvariant() switch
        {
            "json" => new JsonWorldStorage(),
            "xml" => new XmlWorldStorage(),
            "localdb" => new SqlWorldStorage(connectionString),
            "efcore" => throw new NotSupportedException("EfCore storage is not implemented yet."),
            _ => throw new NotSupportedException($"Unsupported storage provider: '{provider}'.")
        };
    }
}