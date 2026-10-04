using System;

namespace CrazyUniverse.Core.Models
{
    public class Lion : Animal
    {
        public Lion(string name) : base(name)
        {
        }

        public override string CrazyAction()
        {
            // TO DO: Implement the crazy action logic for the Lion class
            // Reduce satiety by 10 points when performing a crazy action (as a game rule idea)
            // Satiety -= 10;

            return $"{Name} the lion roars so loud that the whole zoo shakes!";
        }
    }
}