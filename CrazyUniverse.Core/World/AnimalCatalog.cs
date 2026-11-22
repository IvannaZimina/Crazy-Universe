using System;
using System.Collections.Generic;
using System.Linq;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.World
{
    // Describes one supported animal type with its stable class key, fixed enclosure ID, and creation/restore factories
    public sealed class AnimalCatalogEntry
    {
        // Stable class name key (e.g., "Monkey", "Lion") used for saving/loading and routing
        public string TypeKey { get; }

        // Concrete CLR type of the animal subclass
        public Type AnimalClrType { get; }

        // Fixed enclosure (group) identifier associated with this animal type
        public string EnclosureId { get; }

        // Default name used when creating initial animals
        public string DefaultName { get; }

        // Factory delegate for creating a new animal instance
        private readonly Func<string, string?, Animal> _createFactory;

        // Factory delegate for restoring an existing animal instance from saved state
        private readonly Func<string, string, string?, int, Animal> _restoreFactory;

        // Initializes a catalog entry with creation and restoration delegates
        public AnimalCatalogEntry(
            string typeKey,
            Type animalClrType,
            string enclosureId,
            string defaultName,
            Func<string, string?, Animal> createFactory,
            Func<string, string, string?, int, Animal> restoreFactory)
        {
            TypeKey = typeKey ?? throw new ArgumentNullException(nameof(typeKey));
            AnimalClrType = animalClrType ?? throw new ArgumentNullException(nameof(animalClrType));
            EnclosureId = enclosureId ?? throw new ArgumentNullException(nameof(enclosureId));
            DefaultName = defaultName ?? throw new ArgumentNullException(nameof(defaultName));
            _createFactory = createFactory ?? throw new ArgumentNullException(nameof(createFactory));
            _restoreFactory = restoreFactory ?? throw new ArgumentNullException(nameof(restoreFactory));
        }

        // Creates a new animal with a new GUID and default starting satiety
        public Animal CreateNew(string name, string? description = null)
        {
            return _createFactory(name, description);
        }

        // Restores a saved animal using its internal restore constructor with full validation
        public Animal Restore(string id, string name, string? description, int satiety)
        {
            return _restoreFactory(id, name, description, satiety);
        }
    }

    // Central registry of all supported animal types in Core for creation, restoration, and enclosure mapping
    public static class AnimalCatalog
    {
        // All 7 supported animal types and their fixed enclosure IDs
        private static readonly IReadOnlyList<AnimalCatalogEntry> Entries = new List<AnimalCatalogEntry>
        {
            new AnimalCatalogEntry(
                nameof(Monkey),
                typeof(Monkey),
                "enclosure-monkey",
                "George",
                (name, desc) => new Monkey(name, desc),
                (id, name, desc, satiety) => new Monkey(id, name, desc, satiety)),

            new AnimalCatalogEntry(
                nameof(Lion),
                typeof(Lion),
                "enclosure-lion",
                "Simba",
                (name, desc) => new Lion(name, desc),
                (id, name, desc, satiety) => new Lion(id, name, desc, satiety)),

            new AnimalCatalogEntry(
                nameof(Penguin),
                typeof(Penguin),
                "enclosure-penguin",
                "Skipper",
                (name, desc) => new Penguin(name, desc),
                (id, name, desc, satiety) => new Penguin(id, name, desc, satiety)),

            new AnimalCatalogEntry(
                nameof(HoneyBadger),
                typeof(HoneyBadger),
                "enclosure-honeybadger",
                "Rambo",
                (name, desc) => new HoneyBadger(name, desc),
                (id, name, desc, satiety) => new HoneyBadger(id, name, desc, satiety)),

            new AnimalCatalogEntry(
                nameof(Kangaroo),
                typeof(Kangaroo),
                "enclosure-kangaroo",
                "Joey",
                (name, desc) => new Kangaroo(name, desc),
                (id, name, desc, satiety) => new Kangaroo(id, name, desc, satiety)),

            new AnimalCatalogEntry(
                nameof(Dolphin),
                typeof(Dolphin),
                "enclosure-dolphin",
                "Flipper",
                (name, desc) => new Dolphin(name, desc),
                (id, name, desc, satiety) => new Dolphin(id, name, desc, satiety)),

            new AnimalCatalogEntry(
                nameof(Frog),
                typeof(Frog),
                "enclosure-frog",
                "Freddy",
                (name, desc) => new Frog(name, desc),
                (id, name, desc, satiety) => new Frog(id, name, desc, satiety))
        };

        // Lookup dictionaries for fast access by stable type key or concrete CLR type
        private static readonly Dictionary<string, AnimalCatalogEntry> ByTypeKey =
            Entries.ToDictionary(e => e.TypeKey, StringComparer.Ordinal);

        private static readonly Dictionary<Type, AnimalCatalogEntry> ByClrType =
            Entries.ToDictionary(e => e.AnimalClrType);

        // Returns all registered catalog entries
        public static IReadOnlyList<AnimalCatalogEntry> GetAllEntries() => Entries;

        // Returns all supported stable type keys (class names)
        public static IReadOnlyList<string> GetTypeKeys() => Entries.Select(e => e.TypeKey).ToList();

        // Checks whether a type key is present in the catalog
        public static bool IsKnownTypeKey(string? typeKey)
        {
            return !string.IsNullOrWhiteSpace(typeKey) && ByTypeKey.ContainsKey(typeKey.Trim());
        }

        // Returns a catalog entry by type key or throws using localized error text if unknown
        public static AnimalCatalogEntry GetByTypeKey(string typeKey)
        {
            if (!string.IsNullOrWhiteSpace(typeKey) && ByTypeKey.TryGetValue(typeKey.Trim(), out var entry))
            {
                return entry;
            }

            throw new ArgumentException(string.Format(Messages.Err_UnknownAnimalType, typeKey), nameof(typeKey));
        }

        // Returns the catalog entry for an existing Animal instance
        public static AnimalCatalogEntry GetByAnimal(Animal animal)
        {
            ArgumentNullException.ThrowIfNull(animal);

            if (ByClrType.TryGetValue(animal.GetType(), out var entry))
            {
                return entry;
            }

            throw new ArgumentException(string.Format(Messages.Err_UnknownAnimalType, animal.GetType().Name), nameof(animal));
        }

        // Returns the fixed enclosure ID for a concrete animal instance
        public static string GetEnclosureId(Animal animal) => GetByAnimal(animal).EnclosureId;

        // Returns the fixed enclosure ID for a given type key
        public static string GetEnclosureIdByTypeKey(string typeKey) => GetByTypeKey(typeKey).EnclosureId;

        // Creates a new animal by its stable type key
        public static Animal CreateAnimal(string typeKey, string name, string? description = null)
        {
            return GetByTypeKey(typeKey).CreateNew(name, description);
        }

        // Restores a saved animal by its stable type key, ID, name, description, and satiety
        public static Animal RestoreAnimal(string typeKey, string id, string name, string? description, int satiety)
        {
            return GetByTypeKey(typeKey).Restore(id, name, description, satiety);
        }

        // Creates the initial set of default animals (one per type)
        public static List<Animal> CreateDefaultAnimals()
        {
            return Entries.Select(e => e.CreateNew(e.DefaultName, null)).ToList();
        }
    }
}