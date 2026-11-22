using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Storage
{
    // Saves and loads world snapshots as indented UTF-8 JSON using System.Text.Json
    public sealed class JsonWorldStorage : IWorldStorage
    {
        // Shared serializer settings with readable indentation
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        // Writes WorldDto to a UTF-8 JSON file asynchronously
        public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            token.ThrowIfCancellationRequested();

            // Ensure target directory exists when a non-root directory path is specified
            string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Use UTF-8 without BOM for clean cross-platform JSON files
            var utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);
            await using var writer = new StreamWriter(stream, utf8Encoding);

            string json = JsonSerializer.Serialize(world, SerializerOptions);
            await writer.WriteAsync(json.AsMemory(), token).ConfigureAwait(false);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }

        // Reads and deserializes WorldDto from a JSON file; wraps empty or malformed JSON in InvalidWorldDataException
        public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            token.ThrowIfCancellationRequested();

            string content;
            await using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true))
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                content = await reader.ReadToEndAsync(token).ConfigureAwait(false);
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidWorldDataException(Messages.Err_WorldDataNull);
            }

            try
            {
                WorldDto? dto = JsonSerializer.Deserialize<WorldDto>(content, SerializerOptions);
                if (dto is null)
                {
                    throw new InvalidWorldDataException(Messages.Err_WorldDataNull);
                }

                return dto;
            }
            catch (InvalidWorldDataException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                // Preserve original JsonException as InnerException for diagnostics
                throw new InvalidWorldDataException(ex.Message, ex);
            }
        }
    }
}