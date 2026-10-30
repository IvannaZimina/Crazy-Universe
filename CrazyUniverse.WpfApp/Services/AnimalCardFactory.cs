using System.Collections.ObjectModel;
using CrazyUniverse.WpfApp.ViewModels;

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
                    "Pathfinding simulation...",
                    "Calculating optimal routes for banana collection."),

                new SimulationCardViewModel(
                    "Lion",
                    "LIO-002",
                    "Lion",
                    72.0,
                    "/Images/Lions_migration.jpg",
                    "Population simulation...",
                    "Forecasting pride migration and prey population."),

                new SimulationCardViewModel(
                    "Penguin",
                    "PEN-003",
                    "Penguin",
                    45.0,
                    "/Images/penguin_jump.JPG",
                    "Fluid dynamics simulation...",
                    "Calculating underwater movement, temperature and energy costs.")
            };
        }
    }
}
