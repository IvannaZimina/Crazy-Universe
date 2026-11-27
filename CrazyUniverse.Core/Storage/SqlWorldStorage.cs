using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Resources;
using Microsoft.Data.SqlClient;

namespace CrazyUniverse.Core.Storage
{
    // Saves and loads world snapshots in SQL Server / LocalDB using ADO.NET (Microsoft.Data.SqlClient)
    public sealed class SqlWorldStorage : IWorldStorage
    {
        // Constant table and column names to keep SQL statements consistent
        private const string EnclosuresTable = "Enclosures";
        private const string CharactersTable = "Characters";

        private const string ColEnclosureId = "Id";
        private const string ColEnclosureName = "Name";
        private const string ColEnclosureAnimalType = "AnimalType";

        private const string ColCharacterId = "Id";
        private const string ColCharacterType = "Type";
        private const string ColCharacterName = "Name";
        private const string ColCharacterDescription = "Description";
        private const string ColCharacterSatiety = "Satiety";
        private const string ColCharacterEnclosureId = "EnclosureId";

        // Default world schema version stored in database snapshots
        private const int DefaultWorldVersion = 1;

        private readonly string _connectionString;
        private bool _schemaInitialized;

        // Initializes storage with a configured SQL Server / LocalDB connection string
        public SqlWorldStorage(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

            _connectionString = connectionString.Trim();
        }

        // Persists WorldDto inside a single transaction on one connection (path parameter is ignored for SQL storage)
        public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);
            token.ThrowIfCancellationRequested();

            try
            {
                await EnsureDatabaseAndSchemaAsync(token).ConfigureAwait(false);

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(token).ConfigureAwait(false);

                await using var transaction = (SqlTransaction)await connection
                    .BeginTransactionAsync(IsolationLevel.ReadCommitted, token)
                    .ConfigureAwait(false);

                try
                {
                    // Delete child rows (Characters) before parent rows (Enclosures) to respect foreign key constraints
                    await ExecuteNonQueryAsync(
                        connection,
                        transaction,
                        $"DELETE FROM [{CharactersTable}];",
                        token).ConfigureAwait(false);

                    await ExecuteNonQueryAsync(
                        connection,
                        transaction,
                        $"DELETE FROM [{EnclosuresTable}];",
                        token).ConfigureAwait(false);

                    // Insert enclosures first so character foreign keys resolve cleanly
                    foreach (var enclosure in world.Enclosures)
                    {
                        token.ThrowIfCancellationRequested();

                        await using var cmd = connection.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = $@"
                            INSERT INTO [{EnclosuresTable}] ([{ColEnclosureId}], [{ColEnclosureName}], [{ColEnclosureAnimalType}])
                            VALUES (@Id, @Name, @AnimalType);";

                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 100)
                        {
                            Value = (object?)enclosure.Id ?? DBNull.Value
                        });
                        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100)
                        {
                            Value = (object?)enclosure.Name ?? DBNull.Value
                        });
                        cmd.Parameters.Add(new SqlParameter("@AnimalType", SqlDbType.NVarChar, 100)
                        {
                            Value = (object?)enclosure.AnimalType ?? DBNull.Value
                        });

                        await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                    }

                    // Insert characters referencing their enclosure IDs
                    foreach (var character in world.Characters)
                    {
                        token.ThrowIfCancellationRequested();

                        await using var cmd = connection.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = $@"
                            INSERT INTO [{CharactersTable}]
                                ([{ColCharacterId}], [{ColCharacterType}], [{ColCharacterName}], [{ColCharacterDescription}], [{ColCharacterSatiety}], [{ColCharacterEnclosureId}])
                            VALUES
                                (@Id, @Type, @Name, @Description, @Satiety, @EnclosureId);";

                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 64)
                        {
                            Value = (object?)character.Id ?? DBNull.Value
                        });
                        cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar, 100)
                        {
                            Value = (object?)character.Type ?? DBNull.Value
                        });
                        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, Animal.MaxNameLength)
                        {
                            Value = (object?)character.Name ?? DBNull.Value
                        });
                        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, Animal.MaxDescriptionLength)
                        {
                            Value = (object?)character.Description ?? DBNull.Value
                        });
                        cmd.Parameters.Add(new SqlParameter("@Satiety", SqlDbType.Int)
                        {
                            Value = character.Satiety
                        });
                        cmd.Parameters.Add(new SqlParameter("@EnclosureId", SqlDbType.NVarChar, 100)
                        {
                            Value = (object?)character.EnclosureId ?? DBNull.Value
                        });

                        await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                    }

                    await transaction.CommitAsync(token).ConfigureAwait(false);
                }
                catch
                {
                    // Roll back partial changes on any error before rethrowing
                    try
                    {
                        await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore rollback failure so the original exception is preserved
                    }

                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SqlException ex)
            {
                throw new InvalidWorldDataException(ex.Message, ex);
            }
        }

        // Loads enclosures and characters from SQL storage into a WorldDto (path parameter is ignored for SQL storage)
        public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                await EnsureDatabaseAndSchemaAsync(token).ConfigureAwait(false);

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(token).ConfigureAwait(false);

                var enclosures = new List<EnclosureDto>();
                await using (var encCmd = connection.CreateCommand())
                {
                    encCmd.CommandType = CommandType.Text;
                    encCmd.CommandText = $@"
                        SELECT [{ColEnclosureId}], [{ColEnclosureName}], [{ColEnclosureAnimalType}]
                        FROM [{EnclosuresTable}];";

                    await using var reader = await encCmd.ExecuteReaderAsync(token).ConfigureAwait(false);
                    int ordId = reader.GetOrdinal(ColEnclosureId);
                    int ordName = reader.GetOrdinal(ColEnclosureName);
                    int ordType = reader.GetOrdinal(ColEnclosureAnimalType);

                    while (await reader.ReadAsync(token).ConfigureAwait(false))
                    {
                        enclosures.Add(new EnclosureDto
                        {
                            Id = reader.IsDBNull(ordId) ? string.Empty : reader.GetString(ordId),
                            Name = reader.IsDBNull(ordName) ? string.Empty : reader.GetString(ordName),
                            AnimalType = reader.IsDBNull(ordType) ? string.Empty : reader.GetString(ordType)
                        });
                    }
                }

                var characters = new List<CharacterDto>();
                await using (var charCmd = connection.CreateCommand())
                {
                    charCmd.CommandType = CommandType.Text;
                    charCmd.CommandText = $@"
                        SELECT [{ColCharacterId}], [{ColCharacterType}], [{ColCharacterName}],
                               [{ColCharacterDescription}], [{ColCharacterSatiety}], [{ColCharacterEnclosureId}]
                        FROM [{CharactersTable}];";

                    await using var reader = await charCmd.ExecuteReaderAsync(token).ConfigureAwait(false);
                    int ordId = reader.GetOrdinal(ColCharacterId);
                    int ordType = reader.GetOrdinal(ColCharacterType);
                    int ordName = reader.GetOrdinal(ColCharacterName);
                    int ordDesc = reader.GetOrdinal(ColCharacterDescription);
                    int ordSatiety = reader.GetOrdinal(ColCharacterSatiety);
                    int ordEncId = reader.GetOrdinal(ColCharacterEnclosureId);

                    while (await reader.ReadAsync(token).ConfigureAwait(false))
                    {
                        characters.Add(new CharacterDto
                        {
                            Id = reader.IsDBNull(ordId) ? string.Empty : reader.GetString(ordId),
                            Type = reader.IsDBNull(ordType) ? string.Empty : reader.GetString(ordType),
                            Name = reader.IsDBNull(ordName) ? string.Empty : reader.GetString(ordName),
                            Description = reader.IsDBNull(ordDesc) ? null : reader.GetString(ordDesc),
                            Satiety = reader.IsDBNull(ordSatiety) ? 0 : reader.GetInt32(ordSatiety),
                            EnclosureId = reader.IsDBNull(ordEncId) ? string.Empty : reader.GetString(ordEncId)
                        });
                    }
                }

                if (enclosures.Count == 0 && characters.Count == 0)
                {
                    throw new InvalidWorldDataException(Messages.Err_WorldDataNull);
                }

                return new WorldDto
                {
                    Version = DefaultWorldVersion,
                    Enclosures = enclosures,
                    Characters = characters
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidWorldDataException)
            {
                throw;
            }
            catch (SqlException ex)
            {
                throw new InvalidWorldDataException(ex.Message, ex);
            }
        }

        // Ensures the target database and required tables exist before executing save/load operations
        private async Task EnsureDatabaseAndSchemaAsync(CancellationToken token)
        {
            if (_schemaInitialized)
                return;

            var builder = new SqlConnectionStringBuilder(_connectionString);
            string? databaseName = builder.InitialCatalog;

            // If InitialCatalog is specified, connect to master first to create the database if missing
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                var masterBuilder = new SqlConnectionStringBuilder(_connectionString)
                {
                    InitialCatalog = "master"
                };

                await using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
                await masterConnection.OpenAsync(token).ConfigureAwait(false);

                await using var createDbCmd = masterConnection.CreateCommand();
                createDbCmd.CommandType = CommandType.Text;
                createDbCmd.CommandText = @"
                    IF DB_ID(@DatabaseName) IS NULL
                    BEGIN
                        DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName) + N';';
                        EXEC sp_executesql @sql;
                    END";
                createDbCmd.Parameters.Add(new SqlParameter("@DatabaseName", SqlDbType.NVarChar, 128)
                {
                    Value = databaseName
                });

                await createDbCmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }

            await using var targetConnection = new SqlConnection(_connectionString);
            await targetConnection.OpenAsync(token).ConfigureAwait(false);

            string createTablesSql = $@"
                IF OBJECT_ID(N'dbo.[{EnclosuresTable}]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[{EnclosuresTable}] (
                        [{ColEnclosureId}] NVARCHAR(100) NOT NULL PRIMARY KEY,
                        [{ColEnclosureName}] NVARCHAR(100) NOT NULL,
                        [{ColEnclosureAnimalType}] NVARCHAR(100) NOT NULL
                    );
                END;

                IF OBJECT_ID(N'dbo.[{CharactersTable}]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[{CharactersTable}] (
                        [{ColCharacterId}] NVARCHAR(64) NOT NULL PRIMARY KEY,
                        [{ColCharacterType}] NVARCHAR(100) NOT NULL,
                        [{ColCharacterName}] NVARCHAR({Animal.MaxNameLength}) NOT NULL
                            CONSTRAINT [CK_{CharactersTable}_{ColCharacterName}_NonBlank] CHECK (LEN(LTRIM(RTRIM([{ColCharacterName}]))) > 0),
                        [{ColCharacterDescription}] NVARCHAR({Animal.MaxDescriptionLength}) NULL,
                        [{ColCharacterSatiety}] INT NOT NULL
                            CONSTRAINT [CK_{CharactersTable}_{ColCharacterSatiety}_Range] CHECK ([{ColCharacterSatiety}] BETWEEN {Animal.MinSatiety} AND {Animal.MaxSatiety}),
                        [{ColCharacterEnclosureId}] NVARCHAR(100) NOT NULL
                            CONSTRAINT [FK_{CharactersTable}_{EnclosuresTable}] FOREIGN KEY REFERENCES [dbo].[{EnclosuresTable}]([{ColEnclosureId}])
                    );
                END;";

            await ExecuteNonQueryAsync(targetConnection, null, createTablesSql, token).ConfigureAwait(false);
            _schemaInitialized = true;
        }

        private static async Task ExecuteNonQueryAsync(
            SqlConnection connection,
            SqlTransaction? transaction,
            string sql,
            CancellationToken token)
        {
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }
}