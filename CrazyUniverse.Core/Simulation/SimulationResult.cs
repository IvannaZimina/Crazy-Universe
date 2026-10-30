using System;

namespace CrazyUniverse.Core.Simulation
{
    /// =============== DESCRIPTION ===============
    /// Stores the final report of the simulation for a specific animal. 
    /// It keeps the animal's name, final status (Success, Error, or Cancelled), 
    /// the result message or error text, and how long the calculation took.
    /// ===========================================
    public sealed class SimulationResult
    {
        public string AnimalName { get; }
        public string Status { get; set; } // "Success", "Error", "Cancelled"
        public string Details { get; }
        public double ElapsedSeconds { get; }

        public SimulationResult(string animalName, string status, string details, double elapsedSeconds)
        {
            AnimalName = animalName;
            Status = status;
            Details = details;
            ElapsedSeconds = elapsedSeconds;
        }
    }
}