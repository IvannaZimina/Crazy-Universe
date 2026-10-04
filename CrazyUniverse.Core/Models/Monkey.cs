using System;

namespace CrazyUniverse.Core.Models
{
    // inheriting from the abstract Animal base class
    public class Monkey : Animal
    {
        // Constructor that passes the name up to the base Animal constructor using 'base(name)'
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
            // TO DO: Implement the crazy action logic for the Monkey class
            // Reduce satiety by 10 points when performing a crazy action (as a game rule idea)
            // Satiety -= 10;

            return $"{Name} the monkey throws a banana at the visitors!";
        }
    }
}