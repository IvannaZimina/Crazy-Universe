
// This file configures DI services and selects the storage provider at application startup.
// It keeps provider wiring in the composition root instead of inside ViewModels.

using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Persistence.Ef;
using CrazyUniverse.Core.Storage;
using CrazyUniverse.Core.World;
using CrazyUniverse.WpfApp.Services;
using CrazyUniverse.WpfApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace CrazyUniverse.WpfApp;

// Application entry point and DI container setup.
public partial class App : Application
{
    // Root DI container disposed when the app exits.
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Read storage settings from appsettings.json.
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        string provider = configuration["Storage:Provider"]?.Trim() ?? "Json";
        string connectionString = configuration["Storage:ConnectionString"]?.Trim() ?? string.Empty;
        string efConnectionString = ResolveEfConnectionString(configuration, connectionString);

        var services = new ServiceCollection();

        // Register core domain, validation, mapping, and UI dialog services.
        services.AddSingleton<ZooWorld>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<WorldMapper>();
        services.AddSingleton<IFileDialogService, FileDialogService>();

        // Register the chosen IWorldStorage implementation based on Storage:Provider.
        services.AddSingleton<IWorldStorage>(_ => CreateStorage(provider, connectionString, efConnectionString));

        // Inject dependencies into MainViewModel and MainWindow.
        // Passes whether file dialogs are needed instead of leaking provider names into ViewModel.
        bool isFileBasedStorage = IsFileBasedProvider(provider);
        services.AddTransient(sp => new MainViewModel(
            sp.GetRequiredService<ZooWorld>(),
            sp.GetRequiredService<IWorldStorage>(),
            sp.GetRequiredService<WorldMapper>(),
            sp.GetRequiredService<IFileDialogService>(),
            provider,
            isFileBasedStorage));

        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanly dispose the DI container on shutdown.
        _serviceProvider?.Dispose();
        _serviceProvider = null;
        base.OnExit(e);
    }

    // Determines whether the configured provider stores data in files rather than a database.
    private static bool IsFileBasedProvider(string provider)
    {
        return !string.Equals(provider, "LocalDb", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(provider, "EfCore", StringComparison.OrdinalIgnoreCase);
    }

    // Uses dedicated EfConnectionString if set, or points EF to a separate database name.
    private static string ResolveEfConnectionString(IConfiguration configuration, string fallbackConnectionString)
    {
        string configuredEf = configuration["Storage:EfConnectionString"]?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(configuredEf))
        {
            return configuredEf;
        }

        if (fallbackConnectionString.Contains("Database=CrazyUniverseDb", StringComparison.OrdinalIgnoreCase))
        {
            return fallbackConnectionString.Replace("Database=CrazyUniverseDb", "Database=CrazyUniverseEfDb", StringComparison.OrdinalIgnoreCase);
        }

        return fallbackConnectionString;
    }

    // Creates the selected IWorldStorage provider (Json, Xml, LocalDb, or EfCore).
    private static IWorldStorage CreateStorage(string provider, string connectionString, string efConnectionString)
    {
        return provider.ToLowerInvariant() switch
        {
            "json" => new JsonWorldStorage(),
            "xml" => new XmlWorldStorage(),
            "localdb" => new SqlWorldStorage(connectionString),
            "efcore" => CreateEfWorldStorage(efConnectionString),
            _ => throw new NotSupportedException($"Unsupported storage provider: '{provider}'.")
        };
    }

    // Builds IDbContextFactory and EfWorldStorage for the separate EF Core database.
    private static EfWorldStorage CreateEfWorldStorage(string efConnectionString)
    {
        if (string.IsNullOrWhiteSpace(efConnectionString))
        {
            throw new InvalidOperationException("EF Core connection string is missing.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CrazyUniverseDbContext>();
        optionsBuilder.UseSqlServer(efConnectionString);

        // Renamed from PooledDbContextFactory to accurately reflect non-pooled context creation.
        var factory = new SimpleDbContextFactory(optionsBuilder.Options);
        return new EfWorldStorage(factory);
    }

    // Simple IDbContextFactory implementation using preconfigured DbContextOptions.
    private sealed class SimpleDbContextFactory : IDbContextFactory<CrazyUniverseDbContext>
    {
        private readonly DbContextOptions<CrazyUniverseDbContext> _options;

        public SimpleDbContextFactory(DbContextOptions<CrazyUniverseDbContext> options)
        {
            _options = options;
        }

        public CrazyUniverseDbContext CreateDbContext() => new CrazyUniverseDbContext(_options);
    }
}