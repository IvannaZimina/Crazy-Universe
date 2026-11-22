using System.Threading;
using System.Threading.Tasks;
using CrazyUniverse.Core.Persistence;

namespace CrazyUniverse.Core.Storage
{
    // Storage contract for saving and loading serialized world snapshots
    public interface IWorldStorage
    {
        // Saves the world snapshot to the target storage location
        Task SaveAsync(WorldDto world, string path, CancellationToken token = default);

        // Loads and deserializes a world snapshot from the target storage location
        Task<WorldDto> LoadAsync(string path, CancellationToken token = default);
    }
}