using System;
using CrazyUniverse.Core.Interfaces;

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class and implementing ISwimmable interface
    public class Penguin : Animal, ISwimmable
    {
        // Constructor that passes the name up to the base Animal constructor using 'base(name)'
        public Penguin(string name) : base(name)
        {
        }

        // Since the CrazyAction() method was abstract in the parent Animal class, 
        // the Penguin class implements it with its own unique logic.
        public override string CrazyAction()
        {
            // State-changing game rules: decreases satiety and depends on current state
            Satiety -= 10;

            if (Satiety < 20)
            {
                return $"{Name} the penguin is too exhausted and sluggishly slides on the ice! (Current Satiety: {Satiety})";
            }

            return $"{Name} the penguin enthusiastically slides on its belly across the ice! (Current Satiety: {Satiety})";
        }

        // Implementing the ISwimmable interface contract with distance parameter and input validation
        public string Swim(double distance)
        {
            // Protecting object state: invalid input (negative or zero distance) does not change state
            if (distance <= 0)
            {
                return $"Invalid input! Distance must be greater than zero. {Name} stays put, state remains unchanged.";
            }

            // State-changing rule based on the distance parameter[cite: 8]
            Satiety -= (int)distance;

            return $"{Name} the penguin swims gracefully, covering {distance} meters through the cold water! (Current Satiety: {Satiety})";
        }
    }
}