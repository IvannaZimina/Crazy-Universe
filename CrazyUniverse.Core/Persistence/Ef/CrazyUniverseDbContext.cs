// This file defines the EF Core DbContext for Crazy Universe.
// It exposes DbSets for Enclosures and Characters and loads all entity configurations automatically.

using CrazyUniverse.Core.Persistence.Ef.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrazyUniverse.Core.Persistence.Ef;

// EF Core database context used to save and load the Crazy Universe world state.
public sealed class CrazyUniverseDbContext : DbContext
{
    // Receives database options (such as SQL Server connection string) from DI or a factory.
    public CrazyUniverseDbContext(DbContextOptions<CrazyUniverseDbContext> options)
        : base(options)
    {
    }

    // Table of enclosures.
    public DbSet<EnclosureRecord> Enclosures => Set<EnclosureRecord>();

    // Table of characters (TPH hierarchy).
    public DbSet<CharacterRecord> Characters => Set<CharacterRecord>();

    // Automatically finds and applies all IEntityTypeConfiguration classes in this assembly.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrazyUniverseDbContext).Assembly);
    }
}