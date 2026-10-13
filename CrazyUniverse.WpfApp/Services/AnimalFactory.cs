using System.Collections.Generic;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.WpfApp.Factories
{
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