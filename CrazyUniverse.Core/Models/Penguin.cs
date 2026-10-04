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
            // TO DO: Uncomment when state-changing game rules are implemented
            // Satiety -= 10;

            return $"{Name} the penguin slides on its belly across the ice!";
        }

        // Implementing the ISwimmable interface contract
        public string Swim()
        {
            // TO DO: Implement satiety penalty for swimming
            // Satiety -= 4;

            return $"{Name} the penguin swims gracefully through the cold water!";
        }
    }
}