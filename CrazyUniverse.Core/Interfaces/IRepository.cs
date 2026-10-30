using System;
using System.Collections.Generic;
using CrazyUniverse.Core.Events;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.Core.Interfaces
{
    // Generic interface defining repository contract for any type T.
    // Generics (T) allow writing reusable code for different data types securely.
    // 'where T : Animal' - ensures that type T can only be Animal or its subclasses.
    public interface IRepository<T> where T : Animal
    {
        // LogMessageGenerated — an event through which the repository transmits generated text reaction logs to the outside
        event EventHandler<string>? LogMessageGenerated;

        // Method contract for adding new item to repository collection and triggering welcome reactions.
        void Add(T item);

        // Method contract for removing item, triggering farewell reactions, and cleaning up subscriptions.
        bool Remove(T item);

        // Method contract for retrieving all stored items as a protected read-only collection.
        IReadOnlyCollection<T> GetAll();

        // Method contract for searching specific item using condition predicate.
        T? Find(Func<T, bool> predicate);

        // Method contract for triggering a crazy action on a specific animal and collecting neighbor reactions.
        void TriggerCrazyAction(T item);
    }
}