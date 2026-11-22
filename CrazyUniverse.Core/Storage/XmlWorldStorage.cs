using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Storage
{
    // Saves and loads world snapshots as XML using XmlSerializer with async file streams
    public sealed class XmlWorldStorage : IWorldStorage
    {
        private static readonly XmlSerializer Serializer = new(typeof(WorldDto));

        // Writes WorldDto to an XML file asynchronously without duplicate XML declarations
        public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            token.ThrowIfCancellationRequested();

            string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                OmitXmlDeclaration = false,
                Async = true
            };

            // Serialize into MemoryStream first, then strip/avoid duplicate declaration when writing out
            using var memoryStream = new MemoryStream();
            var serializerSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                OmitXmlDeclaration = false
            };

            using (var memXmlWriter = XmlWriter.Create(memoryStream, serializerSettings))
            {
                Serializer.Serialize(memXmlWriter, world);
            }

            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, Encoding.UTF8);
            string xmlText = await reader.ReadToEndAsync(token).ConfigureAwait(false);

            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            await writer.WriteAsync(xmlText.AsMemory(), token).ConfigureAwait(false);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }

        // Reads and deserializes WorldDto from an XML file; wraps empty, root, or parse errors in InvalidWorldDataException
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
                using var stringReader = new StringReader(content);
                using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { IgnoreWhitespace = true });

                // Move past XML declaration/comments/whitespace to the root element
                if (xmlReader.MoveToContent() != XmlNodeType.Element)
                {
                    throw new InvalidWorldDataException(Messages.Err_WorldDataNull);
                }

                object? rawResult = Serializer.Deserialize(xmlReader);
                if (rawResult is not WorldDto dto)
                {
                    throw new InvalidWorldDataException(Messages.Err_WorldDataNull);
                }

                return dto;
            }
            catch (InvalidWorldDataException)
            {
                throw;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException)
            {
                // Preserve XML deserialization/parse exception as InnerException
                throw new InvalidWorldDataException(ex.Message, ex);
            }
        }
    }
}