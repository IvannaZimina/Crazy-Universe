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
            // State-changing game rules implemented
            Satiety -= 20;

            if (Satiety < 20)
            {
                return $"{Name} the lion lets out a weak, tired grumble. (Current Satiety: {Satiety})";
            }

            return $"{Name} the lion roars so loud that the whole zoo shakes! (Current Satiety: {Satiety})";
        }

        // Implementing the IVocalizable interface contract
        public string MakeSound()
        {
            // Satiety penalty for roaring implemented
            Satiety -= 8;

            return $"{Name} the lion lets out a deep, majestic roar! (Current Satiety: {Satiety})";
        }
    }
}