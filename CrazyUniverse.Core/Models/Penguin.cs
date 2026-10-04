using System;

namespace CrazyUniverse.Core.Models
{
    public class Penguin : Animal
    {
        public Penguin(string name) : base(name)
        {
        }

        public override string CrazyAction()
        {
            // TO DO: Implement the crazy action logic for the Penguin class
            // Reduce satiety by 10 points when performing a crazy action (as a game rule idea)
            // Satiety -= 10;

            return $"{Name} the penguin slides on its belly across the ice!";
        }
    }
}