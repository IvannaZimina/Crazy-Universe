using System;
using System.Collections.Generic;
using System.Linq;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;
using CrazyUniverse.Core.World;

namespace CrazyUniverse.Core.Persistence
{
    // Validates deserialized world DTO data before applying any changes to ZooWorld
    public sealed class WorldValidator
    {
        // Currently supported world save schema version
        public const int SupportedVersion = 1;

        // Checks version, enclosures, and every character entry; throws InvalidWorldDataException if invalid
        public void Validate(WorldDto dto)
        {
            if (dto is null)
                throw new InvalidWorldDataException(Messages.Err_WorldDataNull);

            if (dto.Version != SupportedVersion)
                throw new InvalidWorldDataException(
                    string.Format(Messages.Err_UnsupportedWorldVersion, dto.Version, SupportedVersion));

            var catalogEntries = AnimalCatalog.GetAllEntries();
            var catalogByEnclosureId = catalogEntries.ToDictionary(e => e.EnclosureId, StringComparer.Ordinal);
            var catalogByTypeKey = catalogEntries.ToDictionary(e => e.TypeKey, StringComparer.Ordinal);

            if (dto.Enclosures is null || dto.Enclosures.Count == 0)
                throw new InvalidWorldDataException(Messages.Err_EnclosuresMissing);

            // Verify each enclosure exists in the catalog and matches its animal type
            var seenEnclosureIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var enc in dto.Enclosures)
            {
                if (enc is null || string.IsNullOrWhiteSpace(enc.Id))
                    throw new InvalidWorldDataException(Messages.Err_InvalidEnclosure);

                string encId = enc.Id.Trim();
                if (!seenEnclosureIds.Add(encId))
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_DuplicateEnclosureId, encId));

                if (!catalogByEnclosureId.TryGetValue(encId, out var expectedEntry) ||
                    !string.Equals(enc.AnimalType?.Trim(), expectedEntry.TypeKey, StringComparison.Ordinal))
                {
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_EnclosureMismatch, encId, enc.AnimalType));
                }
            }

            if (dto.Characters is null)
                throw new InvalidWorldDataException(Messages.Err_CharactersMissing);

            // Verify IDs, uniqueness, types, names, descriptions, satiety range, and enclosure membership
            var seenCharacterIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ch in dto.Characters)
            {
                if (ch is null)
                    throw new InvalidWorldDataException(Messages.Err_InvalidCharacter);

                if (string.IsNullOrWhiteSpace(ch.Id) || !Guid.TryParse(ch.Id.Trim(), out _))
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_InvalidCharacterId, ch.Id));

                string charId = ch.Id.Trim();
                if (!seenCharacterIds.Add(charId))
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_DuplicateCharacterId, charId));

                if (string.IsNullOrWhiteSpace(ch.Type) || !catalogByTypeKey.TryGetValue(ch.Type.Trim(), out var entry))
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_UnknownAnimalType, ch.Type));

                if (string.IsNullOrWhiteSpace(ch.Name))
                    throw new InvalidWorldDataException(Messages.Err_AnimalNameRequired);

                if (ch.Name.Trim().Length > Animal.MaxNameLength)
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_NameTooLong, Animal.MaxNameLength));

                if (!string.IsNullOrEmpty(ch.Description) && ch.Description.Trim().Length > Animal.MaxDescriptionLength)
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_DescriptionTooLong, Animal.MaxDescriptionLength));

                if (ch.Satiety < Animal.MinSatiety || ch.Satiety > Animal.MaxSatiety)
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_SatietyOutOfRange, ch.Satiety, Animal.MinSatiety, Animal.MaxSatiety));

                string encId = ch.EnclosureId?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(encId) ||
                    !seenEnclosureIds.Contains(encId) ||
                    !string.Equals(encId, entry.EnclosureId, StringComparison.Ordinal))
                {
                    throw new InvalidWorldDataException(
                        string.Format(Messages.Err_InvalidEnclosureForCharacter, ch.Name, ch.EnclosureId, entry.TypeKey));
                }
            }
        }
    }
}