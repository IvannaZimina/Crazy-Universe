namespace CrazyUniverse.Core.Interfaces
{
    public interface IJumpable
    {
        int JumpCost { get; }
        string Jump();
    }
}