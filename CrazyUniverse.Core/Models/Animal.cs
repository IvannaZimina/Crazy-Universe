using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrazyUniverse.Core.Models
{
    // Declare an abstract base class representing any animal in the Crazy Zoo and implement INotifyPropertyChanged
    public abstract class Animal : INotifyPropertyChanged
    {
        // Read-only property for the unique identifier, automatically generated using Guid
        // example: "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
        public string Id { get; } = Guid.NewGuid().ToString();

        private int _satiety;       // Private variable to store the satiety level

        public string Name { get; } // Read-only property for the animal's name, set via constructor

        // Abstract property for the animal's species/type name (e.g., Monkey, Lion, Penguin)
        public abstract string TypeName { get; }

        // Abstract property for a funny description of the animal
        public abstract string Description { get; }

        // Virtual property for feeding cost/gain, can be overridden by specific animals if needed
        public virtual int FeedCost => 5;

        // Encapsulated property: hides the internal value and ensures satiety always stays safely between 0 and 100
        public int Satiety
        {
            get => _satiety;    // getter => Return the current private satiety value
            protected set       // setter => can only be modified internally by the animal's methods
            {
                if (value < 0) _satiety = 0;            // Validate that satiety cannot drop below 0
                else if (value > 100) _satiety = 100;   // Validate that satiety cannot exceed the maximum limit of 100
                else _satiety = value;                  // If the value is within valid bounds, assign it

                // Notify UI elements that Satiety has changed so progress bars update instantly
                OnPropertyChanged();
            }
        }

        // Protected constructor: receives initial data, validates it,
        // sets up the base state (max satiety & auto-generated ID) and initializes the new animal
        protected Animal(string name)
        {
            // make sure the name is not null, empty, or just whitespace
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Animal name is required.", nameof(name));

            Name = name.Trim(); // Clean up any accidental spaces around the name and save it
            Satiety = 100;      // Give every new animal a maximum satiety level of 100 right from the start
        }

        // Virtual method for feeding: adds 5 points if the animal is not fully stuffed
        public virtual string Feed()
        {
            // Check if the animal is already at maximum satiety (100)
            if (Satiety >= 100)
            {
                return $"{Name} is already full (100/100) and cannot eat more!";
            }

            Satiety += FeedCost;   // Increase satiety by 5 points per feeding action

            // Return a descriptive message about the feeding action and current state
            return $"{Name} was fed. Current satiety: {Satiety}/100.";
        }

        // Abstract method: forces every child class (like Monkey or Lion) to create its own unique crazy action
        public abstract string CrazyAction();

        // INotifyPropertyChanged event implementation for WPF data binding updates
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}