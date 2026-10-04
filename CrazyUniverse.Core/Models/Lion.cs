using System;
using CrazyUniverse.Core.Interfaces;

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class and implementing IVocalizable interface
    public class Lion : Animal, IVocalizable
    {
        // Constructor that passes the name up to the base Animal constructor using 'base(name)'
        public Lion(string name) : base(name)
        {
        }

        // Since the CrazyAction() method was abstract in the parent Animal class, 
        // the Lion class implements it with its own unique logic.
        public override string CrazyAction()
        {
            // TO DO: Uncomment when state-changing game rules are implemented
            // Satiety -= 10;

            return $"{Name} the lion roars so loud that the whole zoo shakes!";
        }

        // Implementing the IVocalizable interface contract
        public string MakeSound()
        {
            // TO DO: Implement satiety penalty for roaring
            // Satiety -= 5;

            return $"{Name} the lion lets out a deep, majestic roar!";
        }
    }
}