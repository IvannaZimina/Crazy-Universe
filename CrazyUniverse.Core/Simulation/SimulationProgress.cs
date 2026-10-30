using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyUniverse.Core.Simulation
{
    /// ============== DESCRIPTION ===============
    /// Holds information about the current progress of an animal's simulation task. 
    /// It tracks how many steps are already done out of the total amount, 
    /// and which animal is currently being calculated, so the UI can update smoothly.
    /// ===========================================
    public readonly record struct SimulationProgress(long Done, long Total, string AnimalName);
}