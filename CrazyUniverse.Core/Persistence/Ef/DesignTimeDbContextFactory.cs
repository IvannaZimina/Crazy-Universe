// This file creates CrazyUniverseDbContext for EF Core CLI tools (migrations and database updates).
// It is used at design time when the WPF app and its DI container are not running.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CrazyUniverse.Core.Persistence.Ef;

// Design-time factory so "dotnet ef" commands can build CrazyUniverseDbContext.
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CrazyUniverseDbContext>
{
    // Default LocalDB connection string for the separate EF Core database.
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=CrazyUniverseEfDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

    // Creates a DbContext instance for EF Core migrations and tooling.
    public CrazyUniverseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrazyUniverseDbContext>();
        optionsBuilder.UseSqlServer(DefaultConnectionString);

        return new CrazyUniverseDbContext(optionsBuilder.Options);
    }
}