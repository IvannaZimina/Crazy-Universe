using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;
using System;

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class and implementing IVocalizable interface
    public class Lion : Animal, IVocalizable
    {
        // Implementing abstract properties required by the base Animal class
        public override string TypeName => Messages.UI_LionTypeName;
        public override string Description => Messages.Desc_LionDescription;

        // Implementing the SoundCost property required by the IVocalizable interface (specifies satiety cost for making sound)
        public int SoundCost => 8;

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
                return string.Format(Messages.Action_LionCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_LionCrazyNormal, Name, Satiety);
        }

        // Implementing the IVocalizable interface contract
        public string MakeSound()
        {
            // Satiety penalty for roaring implemented (uses SoundCost)
            Satiety -= SoundCost;

            return string.Format(Messages.Action_LionMakeSound, Name, Satiety);
        }
    }
}