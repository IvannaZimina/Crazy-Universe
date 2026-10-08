using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    public class HoneyBadger : Animal, IClimbable, ISwimmable, IRunnable, IVocalizable
    {
        public override string TypeName => Messages.UI_HoneyBadgerTypeName;

        private string? _customDescription;
        public override string Description
        {
            get => _customDescription ?? Messages.Desc_HoneyBadgerDescription;
            set => _customDescription = value;
        }

        public int ClimbCost => 5;
        public int SoundCost => 2;
        public int SwimCost => 10;
        public int RunCost => 8;

        public HoneyBadger(string name, string? description = null) : base(name, description)
        {
        }

        public override string CrazyAction()
        {
            Satiety -= 15;

            if (Satiety < 20)
            {
                return string.Format(Messages.Action_HoneyBadgerCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_HoneyBadgerCrazyNormal, Name, Satiety);
        }

        public string Climb(double height)
        {
            Satiety -= ClimbCost;
            return string.Format(Messages.Action_HoneyBadgerClimb, Name, Satiety);
        }

        public string MakeSound()
        {
            Satiety -= SoundCost;
            return string.Format(Messages.Action_HoneyBadgerMakeSound, Name, Satiety);
        }

        public string Swim(double distance)
        {
            Satiety -= SwimCost;
            return string.Format(Messages.Action_HoneyBadgerSwim, Name, Satiety);
        }

        public string Run()
        {
            Satiety -= RunCost;
            return string.Format(Messages.Action_HoneyBadgerRun, Name, Satiety);
        }
    }
}