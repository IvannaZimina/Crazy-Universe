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
                new HoneyBadger("Rambo"),
                new Kangaroo("Joey"),
                new Dolphin("Flipper"),
                new Frog("Freddy")
            };
        }

        // Returns technical keys for factory and repository logic
        public static List<string> GetAvailableTypes()
        {
            return new List<string> { "Monkey", "Lion", "Penguin", "HoneyBadger","Kangaroo", "Dolphin", "Frog" };
        }

        // Creates a specific animal based on type key from dialog
        public static Animal CreateAnimal(string animalType, string name, string description)
        {
            return animalType switch
            {
                // after the description set became protected, description must be passed to the constructor
                "Lion" => new Lion(name, description),
                "Penguin" => new Penguin(name, description),
                "HoneyBadger" => new HoneyBadger(name, description),
                "Kangaroo" => new Kangaroo(name, description),
                "Dolphin" => new Dolphin(name, description),
                "Frog" => new Frog(name, description),
                _ => new Monkey(name, description) // "Monkey" is default
            };
        }
    }
}