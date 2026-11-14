using System;

namespace CrazyUniverse.Core.Simulation
{
    /// =============== DESCRIPTION ===============
    /// Saves a copy of the animal's data at the start of the simulation. 
    /// This is needed so that if the user changes anything in the app while the simulation is running, 
    /// calculations won't break and will use the original data.
    /// ===========================================
    public sealed class AnimalSnapshot
    {
        public Guid Id { get; }
        public string Name { get; }
        public string AnimalType { get; }
        public double Satiety { get; }

        public AnimalSnapshot(Guid id, string name, string animalType, double satiety)
        {
            Id = id;
            Name = name;
            AnimalType = animalType;
            Satiety = satiety;
        }
    }
}