// This file implements IWorldStorage using Entity Framework Core.
// It saves world state atomically in a transaction and loads records back into WorldDto.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace CrazyUniverse.Core.Persistence.Ef;

// EF Core implementation of IWorldStorage.
public sealed class EfWorldStorage : IWorldStorage
{
    private readonly IDbContextFactory<CrazyUniverseDbContext> _contextFactory;

    // Receives IDbContextFactory so each save or load gets a fresh DbContext.
    public EfWorldStorage(IDbContextFactory<CrazyUniverseDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    // Saves enclosures and characters as one atomic transaction.
    public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(world);

        var (enclosures, characters) = EfWorldMapper.ToRecords(world);

        await using var db = await _contextFactory.CreateDbContextAsync(token);
        await using var transaction = await db.Database.BeginTransactionAsync(token);

        try
        {
            // Delete characters before enclosures so foreign key constraints are respected.
            await db.Characters.ExecuteDeleteAsync(token);
            await db.Enclosures.ExecuteDeleteAsync(token);

            // Insert the new world snapshot into both tables.
            await db.Enclosures.AddRangeAsync(enclosures, token);
            await db.Characters.AddRangeAsync(characters, token);

            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }
        catch
        {
            // Roll back on any error so partial updates are never kept.
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    // Loads enclosures and characters from EF Core and returns them as a WorldDto.
    public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(token);

        // Read without change tracking since records are converted into WorldDto.
        var enclosures = await db.Enclosures
            .AsNoTracking()
            .ToListAsync(token);

        var characters = await db.Characters
            .AsNoTracking()
            .ToListAsync(token);

        return EfWorldMapper.ToWorldDto(enclosures, characters, WorldValidator.SupportedVersion);
    }
}