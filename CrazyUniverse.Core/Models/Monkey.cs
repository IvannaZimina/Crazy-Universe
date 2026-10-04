using System;
using CrazyUniverse.Core.Interfaces;

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class
    public class Monkey : Animal, IJumpable, IVocalizable
    {
        // Implementing abstract properties required by the base Animal class
        public override string TypeName => "Monkey";
        public override string Description => "A professional chaos enthusiast who believes keyboards are edible and physics is optional. Constantly plots to steal the zookeeper's keys and throw banana peels.";

        // Implementing the JumpCost property required by the IJumpable interface (specifies satiety cost for jumping)
        public int JumpCost => 5;

        // Implementing the SoundCost property required by the IVocalizable interface (specifies satiety cost for making sound)
        public int SoundCost => 2;

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
            // State-changing game rules implemented
            Satiety -= 15;

            if (Satiety < 20)
            {
                return $"{Name} the monkey is too tired to throw anything and sits quietly. (Current Satiety: {Satiety})";
            }

            return $"{Name} the monkey excitedly throws a banana at the visitors! (Current Satiety: {Satiety})";
        }

        // Implementing the IJumpable interface contract
        public string Jump()
        {
            // Satiety penalty for jumping implemented (uses JumpCost)
            Satiety -= JumpCost;

            return $"{Name} the monkey jumps high from branch to branch! (Current Satiety: {Satiety})";
        }

        // Implementing the IVocalizable interface contract
        public string MakeSound()
        {
            // Satiety penalty for making sound implemented (uses SoundCost)
            Satiety -= SoundCost;

            return $"{Name} the monkey chatters loudly: 'Ooh-ooh, aah-aah!' (Current Satiety: {Satiety})";
        }
    }
}