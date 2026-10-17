using CrazyUniverse.Core.Models;
using System;

namespace CrazyUniverse.Core.Events
{
    // ==========================================
    // ANIMAL EVENT ARGS (Domain Event Arguments)
    // ==========================================

    // Carries data for domain events such as Animal Addition, Removal, or CrazyAction within an enclosure.
    public class AnimalEventArgs : EventArgs
    {
        // stores a reference to the animal with which the event occurred
        public Animal TargetAnimal { get; }

        // The type of the event action (e.g., "Add", "Remove", "CrazyAction")
        public string EventType { get; }

        // Constructor to initialize event arguments
        public AnimalEventArgs(Animal targetAnimal, string eventType)
        {
            TargetAnimal = targetAnimal ?? throw new ArgumentNullException(nameof(targetAnimal));
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        }
    }
}