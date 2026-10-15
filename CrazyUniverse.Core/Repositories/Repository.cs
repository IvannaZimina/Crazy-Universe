using System;
using System.Collections.Generic;
using System.Linq;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.Core.Repositories
{
    // Generic class implementing IRepository contract.
    // Stores animals in a private list in memory.
    // 'where T : Animal' - means T can only be Animal or its subclasses.
    public class Repository<T> : IRepository<T> where T : Animal
    {
        // Private list for items of type T.
        private readonly List<T> _items = new();

        // Event triggered when a domain event occurs (addition, removal, crazy action) needing to be logged
        public event EventHandler<string>? LogMessageGenerated;

        // Adds item to internal list, triggers welcome reactions from existing neighbors, and logs them.
        public void Add(T item)
        {
            if (item == null)
                ArgumentNullException.ThrowIfNull(item);

            // Trigger welcome reactions from existing animals in this repository before adding the new one
            foreach (var existingAnimal in _items)
            {
                string reaction = existingAnimal.ReactToNeighbor(item, "Add");
                if (!string.IsNullOrWhiteSpace(reaction))
                {
                    LogMessageGenerated?.Invoke(this, reaction);
                }
            }

            _items.Add(item);
        }

        // Removes the item from the internal list, triggers farewell reactions from remaining neighbors, and returns true if successful.
        public bool Remove(T item)
        {
            if (item == null || !_items.Contains(item))
                return false;

            // Trigger farewell reactions from remaining neighbors before removing the item
            foreach (var remainingAnimal in _items)
            {
                if (remainingAnimal != item)
                {
                    string reaction = remainingAnimal.ReactToNeighbor(item, "Remove");
                    if (!string.IsNullOrWhiteSpace(reaction))
                    {
                        LogMessageGenerated?.Invoke(this, reaction);
                    }
                }
            }

            return _items.Remove(item);
        }

        // Triggers a crazy action on a specific animal and collects neighbor reactions within the same repository.
        public void TriggerCrazyAction(T item)
        {
            if (item == null || !_items.Contains(item))
                return;

            // Perform the animal's own crazy action
            string actionResult = item.CrazyAction();
            LogMessageGenerated?.Invoke(this, actionResult);

            // Trigger neighbor reactions to this crazy action
            foreach (var neighbor in _items)
            {
                if (neighbor != item)
                {
                    string reaction = neighbor.ReactToNeighbor(item, "CrazyAction");
                    if (!string.IsNullOrWhiteSpace(reaction))
                    {
                        LogMessageGenerated?.Invoke(this, reaction);
                    }
                }
            }
        }

        // Returns all items from the list as a new collection.
        public IEnumerable<T> GetAll()
        {
            return _items;
        }

        // Searches the list using a condition and returns the found item or null.
        public T? Find(Func<T, bool> predicate)
        {
            return _items.FirstOrDefault(predicate);
        }
    }
}