
using System.Collections.ObjectModel;
using CrazyUniverse.WpfApp.ViewModels;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.WpfApp.Services
{
    public static class AnimalCardFactory
    {
        public static ObservableCollection<SimulationCardViewModel> CreateDefaultCards()
        {
            return new ObservableCollection<SimulationCardViewModel>
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
    }
}
