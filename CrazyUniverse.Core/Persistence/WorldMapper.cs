using System;
using System.Collections.Generic;
using System.Linq;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.World;

namespace CrazyUniverse.Core.Persistence
{
    // Converts ZooWorld state into WorldDto for saving, and validates/restores WorldDto into Animal instances on load
    public sealed class WorldMapper
    {
        private readonly WorldValidator _validator;

        // Uses the supplied WorldValidator or creates a default one
        public WorldMapper(WorldValidator? validator = null)
        {
            _validator = validator ?? new WorldValidator();
        }

        // Builds a WorldDto snapshot of all enclosures and characters currently in ZooWorld
        public WorldDto ToDto(ZooWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);

            // Build enclosure DTOs from AnimalCatalog entries and localized display names
            var enclosures = AnimalCatalog.GetAllEntries()
                .Select(entry =>
                {
                    var sample = entry.CreateNew(entry.DefaultName, null);
                    return new EnclosureDto
                    {
                        Id = entry.EnclosureId,
                        Name = sample.TypeName,
                        AnimalType = entry.TypeKey
                    };
                })
                .ToList();

            // Export every animal across all repositories with its stable ID, type key, fields, and enclosure ID
            var characters = world.GetAllAnimals()
                .Select(animal =>
                {
                    var entry = AnimalCatalog.GetByAnimal(animal);
                    return new CharacterDto
                    {
                        Id = animal.Id,
                        Type = entry.TypeKey,
                        Name = animal.Name,
                        Description = animal.Description,
                        Satiety = animal.Satiety,
                        EnclosureId = entry.EnclosureId
                    };
                })
                .ToList();

            return new WorldDto
            {
                Version = WorldValidator.SupportedVersion,
                Enclosures = enclosures,
                Characters = characters
            };
        }

        // Validates WorldDto first and reconstructs domain Animal instances through AnimalCatalog
        public IReadOnlyList<Animal> ToAnimals(WorldDto dto)
        {
            // Run full structural and semantic checks before creating any domain objects
            _validator.Validate(dto);

            var result = new List<Animal>(dto.Characters.Count);
            foreach (var ch in dto.Characters)
            {
                try
                {
                    // Restore each animal with its saved ID, name, description, and validated satiety
                    var animal = AnimalCatalog.RestoreAnimal(
                        ch.Type.Trim(),
                        ch.Id.Trim(),
                        ch.Name.Trim(),
                        ch.Description,
                        ch.Satiety);
                    result.Add(animal);
                }
                catch (ArgumentException ex)
                {
                    // Wrap domain constructor/validation errors into InvalidWorldDataException
                    throw new InvalidWorldDataException(ex.Message, ex);
                }
            }

            return result;
        }
    }
}