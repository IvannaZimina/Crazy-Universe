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

        // Adds item to internal list after checking for null reference.
        public void Add(T item)
        {
            if (item == null)
                ArgumentNullException.ThrowIfNull(item);

            _items.Add(item);
        }

        // Removes the item from the internal list and returns true if successful.
        public bool Remove(T item)
        {
            if (item == null)
                return false;

            return _items.Remove(item);
        }

        // Returns all items from the list as a new collection.
        public IEnumerable<T> GetAll()
        {
            return _items;
        }

        //Searches the list using a condition and returns the found item or null.
        public T? Find(Func<T, bool> predicate)
        {
            return _items.FirstOrDefault(predicate);
        }
    }
}