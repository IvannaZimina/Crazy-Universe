using System.Collections.Generic;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.World;

namespace CrazyUniverse.WpfApp.Factories
{
    // Factory class responsible for creating animal instances and managing available types via Core catalog
    public static class AnimalFactory
    {
        // Create and return default collection of animals from Core AnimalCatalog
        public static List<Animal> CreateDefaultAnimals()
        {
            return AnimalCatalog.CreateDefaultAnimals();
        }

        // Returns technical keys for factory and repository logic from Core AnimalCatalog
        public static List<string> GetAvailableTypes()
        {
            return new List<string>(AnimalCatalog.GetTypeKeys());
        }

        // Creates a specific animal based on type key from dialog via Core AnimalCatalog
        public static Animal CreateAnimal(string animalType, string name, string description)
        {
            // after the description set became protected, description must be passed to the constructor
            return AnimalCatalog.CreateAnimal(animalType, name, description);
        }
    }
}