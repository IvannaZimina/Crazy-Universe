using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;
using CrazyUniverse.WpfApp.ViewModels;

namespace CrazyUniverse.WpfApp.Services
{
    /// ==================== DESCRIPTION ====================
    ///  Factory class responsible for creating simulation cards 
    ///  either from default configurations or dynamically from real zoo animals.
    /// =====================================================
    public static class AnimalCardFactory // Defines a static factory utility for building simulation cards.
    {
        // Creates the default hardcoded simulation cards for initial testing or fallback.
        public static ObservableCollection<SimulationCardViewModel> CreateDefaultCards() // Returns a collection of default simulation cards.
        {
            return new ObservableCollection<SimulationCardViewModel> // Creates and populates the observable collection.
            {
                new SimulationCardViewModel(
                    "Monkey",
                    "MON-001",
                    "Monkey",
                    68.0,
                    "/Images/Monkyes_jump.JPG",
                    Messages.Simulation_Card_Monkey_Title,
                    Messages.Simulation_Card_Monkey_Description),

                new SimulationCardViewModel(
                    "Lion",
                    "LIO-002",
                    "Lion",
                    72.0,
                    "/Images/Lions_migration.jpg",
                    Messages.Simulation_Card_Lion_Title,
                    Messages.Simulation_Card_Lion_Description),

                new SimulationCardViewModel(
                    "Penguin",
                    "PEN-003",
                    "Penguin",
                    45.0,
                    "/Images/penguin_jump.JPG",
                    Messages.Simulation_Card_Penguin_Title,
                    Messages.Simulation_Card_Penguin_Description)
            };
        }

        // Creates simulation cards dynamically from real zoo animal instances, satisfying teacher feedback.
        public static IEnumerable<SimulationCardViewModel> CreateCardsFromAnimals(IEnumerable<Animal> animals) // Generates cards from actual active animals in the zoo.
        {
            var cards = new List<SimulationCardViewModel>(); // Initializes a temporary list to hold generated cards.

            foreach (var animal in animals) // Iterates through each real animal provided from the repositories.
            {
                string typeName = animal.GetType().Name; // Gets the exact runtime class name of the animal.

                // Assigns appropriate simulation titles based on the animal type.
                string title = typeName switch
                {
                    "Monkey" => Messages.Simulation_Card_Monkey_Title,
                    "Lion" => Messages.Simulation_Card_Lion_Title,
                    "Penguin" => Messages.Simulation_Card_Penguin_Title,
                    "HoneyBadger" => "Chaos Theory Simulation",
                    "Kangaroo" => "Biomechanics Simulation",
                    "Dolphin" => "Sonar Wave Simulation",
                    "Frog" => "Jump Trajectory Calculation",
                    _ => "General CPU Simulation"
                };

                // Assigns appropriate simulation descriptions based on the animal type.
                string description = typeName switch
                {
                    "Monkey" => Messages.Simulation_Card_Monkey_Description,
                    "Lion" => Messages.Simulation_Card_Lion_Description,
                    "Penguin" => Messages.Simulation_Card_Penguin_Description,
                    "HoneyBadger" => "Simulating unpredictable barrier penetration paths.",
                    "Kangaroo" => "Analyzing kinetic energy, spring force and terrain impact.",
                    "Dolphin" => "Processing echolocation echoes and water turbulence.",
                    "Frog" => "Estimating landing coordinates and hurdle height.",
                    _ => "Running generic intensive mathematical loops."
                };

                // Sets an individual image path corresponding strictly to the animal type.
                string imagePath = typeName switch
                {
                    "Monkey" => "/Images/Monkyes_jump.JPG",
                    "Lion" => "/Images/Lions_migration.jpg",
                    "Penguin" => "/Images/penguin_jump.JPG",
                    "HoneyBadger" => "/Images/honeybadger.jpg",
                    "Kangaroo" => "/Images/kangaroo.jpg",
                    "Dolphin" => "/Images/dolphin.jpg",
                    "Frog" => "/Images/frog.jpg",
                    _ => "/Images/Monkyes_jump.JPG"
                };

                // Adds a new card configured with the real animal's properties (Name, ID, type, satiety).
                cards.Add(new SimulationCardViewModel(
                    animal.Name,                                // Uses the actual name given by the user.
                    animal.Id.ToString()[..8].ToUpper(),        // Uses a shortened prefix of the real animal ID.
                    typeName,                                   // Uses the real concrete animal type.
                    animal.Satiety,                             // Uses the real current satiety value of the animal.
                    imagePath,                                  // Uses the matching individual image path.
                    title,                                      // Uses the assigned simulation title.
                    description                                 // Uses the assigned simulation description.
                ));
            }

            return cards; // Returns the collection of cards created from real animals.
        }
    }
}