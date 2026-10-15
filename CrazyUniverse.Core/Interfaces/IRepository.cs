using System;
using System.Collections.Generic;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.Core.Interfaces
{
    // Generic interface defining repository contract for any type T.
    // Generics (T) allow writing reusable code for different data types securely.
    // 'where T : Animal' - ensures that type T can only be Animal or its subclasses.
    public interface IRepository<T> where T : Animal
    {
        // Method contract for adding new item to repository collection.
        void Add(T item);

        // Method contract for removing item and returning true if successful.
        bool Remove(T item);

        // Method contract for retrieving all stored items as collection.
        IEnumerable<T> GetAll();

        // Method contract for searching specific item using condition predicate.
        T? Find(Func<T, bool> predicate);
    }
}