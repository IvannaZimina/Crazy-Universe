using System;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources; // Подключаем пространство имен с ресурсами

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class
    public class HoneyBadger : Animal, IClimbable, ISwimmable, IRunnable , IVocalizable
    {
        // Implementing abstract properties required by the base Animal class
        public override string TypeName => Messages.UI_HoneyBadgerTypeName;
        public override string Description => Messages.Desc_HoneyBadgerDescription;

        // Implementing the ClimbCost property required by the IClimbable interface (specifies satiety cost for climbing)
        public int ClimbCost => 5;

        // Implementing the SoundCost property required by the IVocalizable interface (specifies satiety cost for making sound)
        public int SoundCost => 2;

        // Implementing the SwimCost property required by the ISwimmable interface (specifies satiety cost for swimming)
        public int SwimCost => 10;

        // Implementing the RunCost property required by the IRunnable interface (specifies satiety cost for running)
        public int RunCost => 8; 

        // Constructor that passes the name up to the base Animal constructor using 'base(name)'
        public HoneyBadger(string name) : base(name)
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
                return string.Format(Messages.Action_HoneyBadgerCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_HoneyBadgerCrazyNormal, Name, Satiety);
        }

        // Implementing the IClimbable interface contract
        public string Climb(double height)
        {
            // Satiety penalty for climbing implemented (uses ClimbCost)
            Satiety -= ClimbCost;

            return string.Format(Messages.Action_HoneyBadgerClimb, Name, Satiety);
        }

        // Implementing the IVocalizable interface contract
        public string MakeSound()
        {
            // Satiety penalty for making sound implemented (uses SoundCost)
            Satiety -= SoundCost;

            return string.Format(Messages.Action_HoneyBadgerMakeSound, Name, Satiety);
        }

        // Implementing the ISwimmable interface contract
        public string Swim(double distance)
        {
            // Satiety penalty for swimming implemented (uses SwimCost)
            Satiety -= SwimCost;

            return string.Format(Messages.Action_HoneyBadgerSwim, Name, Satiety);
        }

        public string Run()
        {
            // Satiety penalty for running implemented
            Satiety -= 8;
            return string.Format(Messages.Action_HoneyBadgerRun, Name, Satiety);
        }
    }
}