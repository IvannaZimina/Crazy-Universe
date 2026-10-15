using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    /// <summary>
    /// Represents a dolphin that can swim and make sounds.
    /// </summary>
    public class Dolphin : Animal, ISwimmable, IVocalizable
    {
        public override string TypeName => Messages.UI_DolphinTypeName;

        private string? _customDescription;

        public override string Description
        {
            get => _customDescription ?? Messages.Desc_DolphinDescription;
            set => _customDescription = value;
        }

        public int SwimCost => 2;

        public int SoundCost => 4;

        /// <summary>
        /// Initializes a new dolphin with the specified name and optional description.
        /// </summary>
        public Dolphin(string name, string? description = null)
            : base(name, description)
        {
        }

        /// <summary>
        /// Performs the dolphin's unique crazy action based on its satiety.
        /// </summary>
        public override string CrazyAction()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= 14;

            if (Satiety < 20)
            {
                return string.Format(Messages.Action_DolphinCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_DolphinCrazyNormal, Name, Satiety);
        }

        /// <summary>
        /// Makes the dolphin swim the specified distance and reduces its satiety.
        /// </summary>
        public string Swim(double distance)
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            if (distance <= 0)
            {
                return string.Format(Messages.Err_InvalidDistance, Name);
            }

            Satiety -= (int)(distance * SwimCost);

            return string.Format(Messages.Action_DolphinSwim, Name, distance, Satiety);
        }

        /// <summary>
        /// Makes the dolphin produce a sound and reduces its satiety.
        /// </summary>
        public string MakeSound()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= SoundCost;

            return string.Format(Messages.Action_DolphinMakeSound, Name, Satiety);
        }
    }
}