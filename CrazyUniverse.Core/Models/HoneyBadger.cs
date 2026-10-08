using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    /// <summary>
    /// A tiny, furious legend who does not care. Climbs, swims, runs and growls,
    /// and every action costs satiety.
    /// </summary>
    public class HoneyBadger : Animal, IClimbable, ISwimmable, IRunnable, IVocalizable
    {
        /// <inheritdoc/>
        public override string TypeName => Messages.UI_HoneyBadgerTypeName;

        /// <inheritdoc/>
        public override string Description => Messages.Desc_HoneyBadgerDescription;

        /// <summary>Satiety cost of climbing.</summary>
        public int ClimbCost => 5;

        /// <summary>Satiety cost of making a sound.</summary>
        public int SoundCost => 2;

        /// <summary>Satiety cost of swimming.</summary>
        public int SwimCost => 10;

        /// <summary>Satiety cost of running.</summary>
        public int RunCost => 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoneyBadger"/> class.
        /// </summary>
        /// <param name="name">The name of the honey badger.</param>
        public HoneyBadger(string name) : base(name)
        {
        }

        /// <summary>
        /// Performs a crazy action. The message depends on the remaining satiety.
        /// </summary>
        /// <returns>A message describing what the honey badger did.</returns>
        public override string CrazyAction()
        {
            Satiety -= 15;

            if (Satiety < 20)
            {
                return string.Format(Messages.Action_HoneyBadgerCrazyLow, Name, Satiety);
            }

            return string.Format(Messages.Action_HoneyBadgerCrazyNormal, Name, Satiety);
        }

        /// <summary>
        /// Climbs to the given height and lowers satiety by <see cref="ClimbCost"/>.
        /// </summary>
        /// <param name="height">The height to climb.</param>
        /// <returns>A message describing the climb.</returns>
        public string Climb(double height)
        {
            Satiety -= ClimbCost;
            return string.Format(Messages.Action_HoneyBadgerClimb, Name, Satiety);
        }

        /// <summary>
        /// Growls and lowers satiety by <see cref="SoundCost"/>.
        /// </summary>
        /// <returns>A message describing the growl.</returns>
        public string MakeSound()
        {
            Satiety -= SoundCost;
            return string.Format(Messages.Action_HoneyBadgerMakeSound, Name, Satiety);
        }

        /// <summary>
        /// Swims the given distance and lowers satiety by <see cref="SwimCost"/>.
        /// </summary>
        /// <param name="distance">The distance to swim.</param>
        /// <returns>A message describing the swim.</returns>
        public string Swim(double distance)
        {
            Satiety -= SwimCost;
            return string.Format(Messages.Action_HoneyBadgerSwim, Name, Satiety);
        }

        /// <summary>
        /// Runs and lowers satiety by <see cref="RunCost"/>.
        /// </summary>
        /// <returns>A message describing the run.</returns>
        public string Run()
        {
            Satiety -= RunCost;
            return string.Format(Messages.Action_HoneyBadgerRun, Name, Satiety);
        }
    }
}