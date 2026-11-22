using System;
using System.Collections.Generic;
using System.Linq;
using CrazyUniverse.Core.Events;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Repositories;

namespace CrazyUniverse.Core.World
{
    // Owns the type-based repositories and routes domain operations and events inside Core
    public sealed class ZooWorld
    {
        private readonly IRepository<Monkey> _monkeyRepository = new Repository<Monkey>();
        private readonly IRepository<Lion> _lionRepository = new Repository<Lion>();
        private readonly IRepository<Penguin> _penguinRepository = new Repository<Penguin>();
        private readonly IRepository<HoneyBadger> _honeyBadgerRepository = new Repository<HoneyBadger>();
        private readonly IRepository<Kangaroo> _kangarooRepository = new Repository<Kangaroo>();
        private readonly IRepository<Dolphin> _dolphinRepository = new Repository<Dolphin>();
        private readonly IRepository<Frog> _frogRepository = new Repository<Frog>();

        // Internal handler table keyed by stable class name so ViewModels never branch on animal types
        private readonly Dictionary<string, IAnimalRepositoryAdapter> _adapters;

        // Single forwarded event carrying domain log messages from all repositories
        public event EventHandler<LogEventArgs>? LogMessageGenerated;

        public ZooWorld()
        {
            _adapters = new Dictionary<string, IAnimalRepositoryAdapter>(StringComparer.Ordinal)
            {
                [nameof(Monkey)] = new TypedRepositoryAdapter<Monkey>(_monkeyRepository, ForwardLog),
                [nameof(Lion)] = new TypedRepositoryAdapter<Lion>(_lionRepository, ForwardLog),
                [nameof(Penguin)] = new TypedRepositoryAdapter<Penguin>(_penguinRepository, ForwardLog),
                [nameof(HoneyBadger)] = new TypedRepositoryAdapter<HoneyBadger>(_honeyBadgerRepository, ForwardLog),
                [nameof(Kangaroo)] = new TypedRepositoryAdapter<Kangaroo>(_kangarooRepository, ForwardLog),
                [nameof(Dolphin)] = new TypedRepositoryAdapter<Dolphin>(_dolphinRepository, ForwardLog),
                [nameof(Frog)] = new TypedRepositoryAdapter<Frog>(_frogRepository, ForwardLog)
            };
        }

        public IReadOnlyList<string> GetAvailableTypeKeys() => AnimalCatalog.GetTypeKeys();

        // Adds an animal to its matching type repository and triggers neighbor reactions
        public void Add(Animal animal, bool suppressLog = false)
        {
            ArgumentNullException.ThrowIfNull(animal);
            GetAdapterForAnimal(animal).Add(animal, suppressLog);
        }

        // Removes an animal from its matching type repository and triggers neighbor reactions
        public bool Remove(Animal animal)
        {
            if (animal == null)
                return false;

            return GetAdapterForAnimal(animal).Remove(animal);
        }

        // Triggers CrazyAction on an animal through its type repository so enclosure neighbors react
        public void TriggerCrazyAction(Animal animal)
        {
            if (animal == null)
                return;

            GetAdapterForAnimal(animal).TriggerCrazyAction(animal);
        }

        // Silently and atomically replaces repository contents without neighbor reactions or log events
        public void Restore(IEnumerable<Animal> animals)
        {
            ArgumentNullException.ThrowIfNull(animals);

            // Validate all incoming animals and duplicate IDs before mutating any repository
            var materialised = animals.ToList();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var animal in materialised)
            {
                ArgumentNullException.ThrowIfNull(animal);

                if (!seenIds.Add(animal.Id))
                {
                    throw new InvalidOperationException($"Duplicate animal ID during world restore: {animal.Id}");
                }

                // Ensure the animal's concrete type is known to the world adapters
                _ = GetAdapterForAnimal(animal);
            }

            // Group validated animals by stable class name so each repository receives only its matching type
            var groupedByType = materialised
                .GroupBy(a => a.GetType().Name, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

            foreach (var pair in _adapters)
            {
                groupedByType.TryGetValue(pair.Key, out var itemsForType);
                pair.Value.ReplaceAll(itemsForType ?? Enumerable.Empty<Animal>());
            }
        }

        // Returns all animals across all repositories
        public IReadOnlyList<Animal> GetAllAnimals()
        {
            return _adapters.Values
                .SelectMany(adapter => adapter.GetAnimals())
                .ToList();
        }

        // Returns animals belonging to a specific type key (e.g., "Monkey", "Lion")
        public IReadOnlyList<Animal> GetAnimalsByType(string typeKey)
        {
            if (!string.IsNullOrWhiteSpace(typeKey) && _adapters.TryGetValue(typeKey.Trim(), out var adapter))
            {
                return adapter.GetAnimals().ToList();
            }

            return Array.Empty<Animal>();
        }

        // Exposes typed repositories when direct generic access is needed
        public IRepository<T> GetRepository<T>() where T : Animal
        {
            return typeof(T) switch
            {
                var t when t == typeof(Monkey) => (IRepository<T>)_monkeyRepository,
                var t when t == typeof(Lion) => (IRepository<T>)_lionRepository,
                var t when t == typeof(Penguin) => (IRepository<T>)_penguinRepository,
                var t when t == typeof(HoneyBadger) => (IRepository<T>)_honeyBadgerRepository,
                var t when t == typeof(Kangaroo) => (IRepository<T>)_kangarooRepository,
                var t when t == typeof(Dolphin) => (IRepository<T>)_dolphinRepository,
                var t when t == typeof(Frog) => (IRepository<T>)_frogRepository,
                _ => throw new NotSupportedException($"Unsupported animal type: {typeof(T).Name}")
            };
        }

        private void ForwardLog(object? sender, LogEventArgs e)
        {
            LogMessageGenerated?.Invoke(this, e);
        }

        private IAnimalRepositoryAdapter GetAdapterForAnimal(Animal animal)
        {
            string typeKey = animal.GetType().Name;
            if (_adapters.TryGetValue(typeKey, out var adapter))
            {
                return adapter;
            }

            throw new NotSupportedException($"Unsupported animal type: {typeKey}");
        }

        private interface IAnimalRepositoryAdapter
        {
            void Add(Animal animal, bool suppressLog);
            bool Remove(Animal animal);
            void TriggerCrazyAction(Animal animal);
            IEnumerable<Animal> GetAnimals();
            void ReplaceAll(IEnumerable<Animal> items);
        }

        private sealed class TypedRepositoryAdapter<T> : IAnimalRepositoryAdapter where T : Animal
        {
            private readonly IRepository<T> _repository;

            public TypedRepositoryAdapter(IRepository<T> repository, EventHandler<LogEventArgs> logForwarder)
            {
                _repository = repository;
                _repository.LogMessageGenerated += logForwarder;
            }

            public void Add(Animal animal, bool suppressLog)
            {
                if (animal is T typed)
                {
                    _repository.Add(typed, suppressLog);
                }
            }

            public bool Remove(Animal animal)
            {
                return animal is T typed && _repository.Remove(typed);
            }

            public void TriggerCrazyAction(Animal animal)
            {
                if (animal is T typed)
                {
                    _repository.TriggerCrazyAction(typed);
                }
            }

            public IEnumerable<Animal> GetAnimals()
            {
                return _repository.GetAll().Cast<Animal>();
            }

            public void ReplaceAll(IEnumerable<Animal> items)
            {
                _repository.ReplaceAll(items.Cast<T>());
            }
        }
    }
}