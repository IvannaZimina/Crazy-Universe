using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    /// <summary>
    /// Represents a frog that can jump and swim.
    /// </summary>
    public class Frog : Animal, IJumpable, ISwimmable
    {
        public override string TypeName => Messages.UI_FrogTypeName;

        private string? _customDescription;

        public override string Description
        {
            get => _customDescription ?? Messages.Desc_FrogDescription;
            set => _customDescription = value;
        }

        public int JumpCost => 4;

        public int SwimCost => 1;

        /// <summary>
        /// Initializes a new frog with the specified name and optional description.
        /// </summary>
        public Frog(string name, string? description = null)
            : base(name, description)
        {
        }

        /// <summary>
        /// Performs the frog's unique crazy action based on its satiety.
        /// </summary>
        public override string CrazyAction()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= 9;

            if (Satiety < 20)
            {
                return string.Format(Messages.Action_FrogCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_FrogCrazyNormal, Name, Satiety);
        }

        /// <summary>
        /// Makes the frog jump and reduces its satiety.
        /// </summary>
        public string Jump()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= JumpCost;

            return string.Format(Messages.Action_FrogJump, Name, Satiety);
        }

        /// <summary>
        /// Makes the frog swim the specified distance and reduces its satiety.
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

            return string.Format(Messages.Action_FrogSwim, Name, distance, Satiety);
        }
    }
}