using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    /// <summary>
    /// Represents a kangaroo that can jump and make sounds.
    /// </summary>
    public class Kangaroo : Animal, IJumpable, IVocalizable
    {
        public override string TypeName => Messages.UI_KangarooTypeName;

        private string? _customDescription;

        public override string Description
        {
            get => _customDescription ?? Messages.Desc_KangarooDescription;
            set => _customDescription = value;
        }

        public int JumpCost => 7;

        public int SoundCost => 3;

        /// <summary>
        /// Initializes a new kangaroo with the specified name and optional description.
        /// </summary>
        public Kangaroo(string name, string? description = null)
            : base(name, description)
        {
        }

        /// <summary>
        /// Performs the kangaroo's unique crazy action based on its satiety.
        /// </summary>
        public override string CrazyAction()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= 18;

            if (Satiety < 20)
            {
                return string.Format(Messages.Action_KangarooCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_KangarooCrazyNormal, Name, Satiety);
        }

        /// <summary>
        /// Makes the kangaroo jump and reduces its satiety.
        /// </summary>
        public string Jump()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= JumpCost;

            return string.Format(Messages.Action_KangarooJump, Name, Satiety);
        }

        /// <summary>
        /// Makes the kangaroo produce a sound and reduces its satiety.
        /// </summary>
        public string MakeSound()
        {
            if (Satiety <= 0)
            {
                return string.Format(Messages.Action_NoEnergy, Name);
            }

            Satiety -= SoundCost;

            return string.Format(Messages.Action_KangarooMakeSound, Name, Satiety);
        }
    }
}