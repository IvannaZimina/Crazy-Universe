using System;
using CrazyUniverse.Core.Interfaces;

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class
    public class Monkey : Animal, IJumpable, IVocalizable
    {
        // Constructor that passes the name up toeman base Animal constructor using 'base(name)'
        public Monkey(string name) : base(name)
        {
            // The ': base(name)' part takes the name received here and sends it up 
            // to the parent Animal constructor, making sure the base class handles 
            // name validation, ID generation, and sets the starting satiety to 100.
        }

        // Since the CrazyAction() method was abstract (empty) in the parent Animal class, 
        // the Monkey class is required to implement it with its own unique logic.
        // override - keyword meaning "redefine" or "fill in". 
        public override string CrazyAction()
        {
            // TO DO: Uncomment when state-changing game rules are implemented
            // Satiety -= 10;

            return $"{Name} the monkey throws a banana at the visitors!";
        }

        // Implementing the IJumpable interface contract
        public string Jump()
        {
            // TO DO: Implement satiety penalty for jumping
            // Satiety -= 5;

            return $"{Name} the monkey jumps high from branch to branch!";
        }

        // Implementing the IVocalizable interface contract
        public string MakeSound()
        {
            // TO DO: Implement satiety penalty for making sound
            // Satiety -= 2;

            return $"{Name} the monkey chatters loudly: 'Ooh-ooh, aah-aah!'";
        }
    }
}