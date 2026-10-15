using System.Collections.Generic;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.WpfApp.Factories
{
    // Factory class responsible for creating animal instances and managing available types
    public static class AnimalFactory
    {
        // Create and return default collection of animals
        public static List<Animal> CreateDefaultAnimals()
        {
            return new List<Animal>
            {
                new Monkey("George"),
                new Lion("Simba"),
                new Penguin("Skipper"),
                new HoneyBadger("Rambo")
            };
        }

        // Returns technical keys for factory and repository logic
        public static List<string> GetAvailableTypes()
        {
            return new List<string> { "Monkey", "Lion", "Penguin", "HoneyBadger" };
        }

        // Creates a specific animal based on type key from dialog
        public static Animal CreateAnimal(string animalType, string name, string description)
        {
            Animal animal = animalType switch
            {
                "Lion" => new Lion(name),
                "Penguin" => new Penguin(name),
                "HoneyBadger" => new HoneyBadger(name),
                _ => new Monkey(name) // "Monkey" is default
            };

            if (!string.IsNullOrWhiteSpace(description))
            {
                animal.Description = description;
            }

            return animal;
        }
    }
}