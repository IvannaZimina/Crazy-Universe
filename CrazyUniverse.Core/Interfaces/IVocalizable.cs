namespace CrazyUniverse.Core.Interfaces
{
    public interface IVocalizable
    {
        int SoundCost { get; }
        string MakeSound();
    }
}