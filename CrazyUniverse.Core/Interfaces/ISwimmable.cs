namespace CrazyUniverse.Core.Interfaces
{
    public interface ISwimmable
    {
        int SwimCost { get; }
        string Swim(double distance);
    }
}