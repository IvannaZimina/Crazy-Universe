using System;
using System.Collections.Generic;
using System.Linq;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.Core.World
{
    // Describes a single animal type in the catalog with a stable class-based type key and fixed enclosure id
    public sealed class AnimalCatalogEntry
    {
        public string TypeKey { get; }
        public Type AnimalClrType { get; }
        public string EnclosureId { get; }
        public string DefaultName { get; }
        private readonly Func<string, string?, Animal> _factoryMethod;

        public AnimalCatalogEntry(
            string typeKey,
            Type animalClrType,
            string enclosureId,
            string defaultName,
            Func<string, string?, Animal> factoryMethod)
        {
            TypeKey = typeKey ?? throw new ArgumentNullException(nameof(typeKey));
            AnimalClrType = animalClrType ?? throw new ArgumentNullException(nameof(animalClrType));
            EnclosureId = enclosureId ?? throw new ArgumentNullException(nameof(enclosureId));
            DefaultName = defaultName ?? throw new ArgumentNullException(nameof(defaultName));
            _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
        }

        // Creates a new animal instance with a freshly generated Id and default starting satiety
        public Animal CreateNew(string name, string? description = null)
        {
            return _factoryMethod(name, description);
        }

        // Restores an animal instance preserving its persisted Id, Name, Description, and Satiety
        public Animal Restore(string id, string name, string? description, int satiety)
        {
            Animal animal = _factoryMethod(name, description);
            animal.RestoreState(id, satiety);
            return animal;
        }
    }

    // Central catalog of supported animal types in Core for creation, restoration, and enclosure mapping
    public static class AnimalCatalog
    {
        private static readonly IReadOnlyList<AnimalCatalogEntry> Entries = new List<AnimalCatalogEntry>
        {
            new AnimalCatalogEntry(
                nameof(Monkey),
                typeof(Monkey),
                "enclosure-monkey",
                "George",
                (name, desc) => new Monkey(name, desc)),

            new AnimalCatalogEntry(
                nameof(Lion),
                typeof(Lion),
                "enclosure-lion",
                "Simba",
                (name, desc) => new Lion(name, desc)),

            new AnimalCatalogEntry(
                nameof(Penguin),
                typeof(Penguin),
                "enclosure-penguin",
                "Skipper",
                (name, desc) => new Penguin(name, desc)),

            new AnimalCatalogEntry(
                nameof(HoneyBadger),
                typeof(HoneyBadger),
                "enclosure-honeybadger",
                "Rambo",
                (name, desc) => new HoneyBadger(name, desc)),

            new AnimalCatalogEntry(
                nameof(Kangaroo),
                typeof(Kangaroo),
                "enclosure-kangaroo",
                "Joey",
                (name, desc) => new Kangaroo(name, desc)),

            new AnimalCatalogEntry(
                nameof(Dolphin),
                typeof(Dolphin),
                "enclosure-dolphin",
                "Flipper",
                (name, desc) => new Dolphin(name, desc)),

            new AnimalCatalogEntry(
                nameof(Frog),
                typeof(Frog),
                "enclosure-frog",
                "Freddy",
                (name, desc) => new Frog(name, desc))
        };

        private static readonly Dictionary<string, AnimalCatalogEntry> ByTypeKey =
            Entries.ToDictionary(e => e.TypeKey, StringComparer.Ordinal);

        private static readonly Dictionary<Type, AnimalCatalogEntry> ByClrType =
            Entries.ToDictionary(e => e.AnimalClrType);

        public static IReadOnlyList<AnimalCatalogEntry> GetAllEntries() => Entries;

        public static IReadOnlyList<string> GetTypeKeys() => Entries.Select(e => e.TypeKey).ToList();

        public static AnimalCatalogEntry GetByTypeKey(string typeKey)
        {
            if (!string.IsNullOrWhiteSpace(typeKey) && ByTypeKey.TryGetValue(typeKey.Trim(), out var entry))
            {
                return entry;
            }

            // Fallback to the first registered catalog entry (Monkey)
            return Entries[0];
        }

        public static AnimalCatalogEntry GetByAnimal(Animal animal)
        {
            ArgumentNullException.ThrowIfNull(animal);
            return ByClrType.TryGetValue(animal.GetType(), out var entry)
                ? entry
                : Entries[0];
        }

        public static string GetEnclosureId(Animal animal) => GetByAnimal(animal).EnclosureId;

        public static string GetEnclosureIdByTypeKey(string typeKey) => GetByTypeKey(typeKey).EnclosureId;

        public static Animal CreateAnimal(string typeKey, string name, string? description = null)
        {
            return GetByTypeKey(typeKey).CreateNew(name, description);
        }

        public static Animal RestoreAnimal(string typeKey, string id, string name, string? description, int satiety)
        {
            return GetByTypeKey(typeKey).Restore(id, name, description, satiety);
        }

        public static List<Animal> CreateDefaultAnimals()
        {
            return Entries.Select(e => e.CreateNew(e.DefaultName, null)).ToList();
        }
    }
}