using System;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    // Inheriting from the abstract Animal base class and implementing ISwimmable interface
    public class Penguin : Animal, ISwimmable
    {
        // Implementing abstract properties required by the base Animal class
        public override string TypeName => Messages.UI_PenguinTypeName;

        private string? _customDescription;
        public override string Description
        {
            get => _customDescription ?? Messages.Desc_PenguinDescription;
            set => _customDescription = value;
        }

        // Implementing the SwimCost property required by the ISwimmable interface (base satiety cost per meter of swimming)
        public int SwimCost => 1;

        // Constructor that passes the name and optional description up to the base Animal constructor
        public Penguin(string name, string? description = null) : base(name, description)
        {
        }

        // Since the CrazyAction() method was abstract in the parent Animal class, 
        // the Penguin class implements it with its own unique logic.
        public override string CrazyAction()
        {
            // State-changing game rules: decreases satiety and depends on current state
            Satiety -= 10;

            if (Satiety < 20)
            {
                return string.Format(Messages.Action_PenguinCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_PenguinCrazyNormal, Name, Satiety);
        }

        // Implementing the ISwimmable interface contract with distance parameter and input validation
        public string Swim(double distance)
        {
            // Protecting object state: invalid input (negative or zero distance) does not change state
            if (distance <= 0)
            {
                return string.Format(Messages.Err_InvalidDistance, Name);
            }

            // State-changing rule based on the distance parameter multiplied by SwimCost
            Satiety -= (int)(distance * SwimCost);

            return string.Format(Messages.Action_PenguinSwim, Name, distance, Satiety);
        }
    }
}