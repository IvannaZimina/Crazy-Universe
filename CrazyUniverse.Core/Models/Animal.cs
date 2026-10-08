using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CrazyUniverse.Core.Resources;

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
                throw new ArgumentException(Messages.Err_AnimalNameRequired, nameof(name));

            Name = name.Trim(); // Clean up any accidental spaces around the name and save it
            Satiety = 100;      // Give every new animal a maximum satiety level of 100 right from the start
        }

        // Virtual method for feeding: adds 5 points if the animal is not fully stuffed
        public virtual string Feed()
        {
            // Check if the animal is already at maximum satiety (100)
            if (Satiety >= 100)
            {
                return string.Format(Messages.Action_FeedAlreadyFull, Name);
            }

            Satiety += FeedCost;   // Increase satiety by 5 points per feeding action

            // Return a descriptive message about the feeding action and current state
            return string.Format(Messages.Action_FeedSuccess, Name, Satiety);
        }

        // Abstract method: forces every child class (like Monkey or Lion) to create its own unique crazy action
        public abstract string CrazyAction();

        // INotifyPropertyChanged event implementation for WPF data binding updates

        // [event PropertyChangedEventHandler:] is a special system event from the System.ComponentModel namespace.
        // WPF listens to this event to know when to redraw the screen.

        // Old method of fall protection:
        // var handler = PropertyChanged;
        // if (handler != null) {
        //     handler(this, new PropertyChangedEventArgs(propertyName));
        // ==}

        // the new one: [= delegate { };] => makes it so that the PropertyChanged variable is never null. It always contains an "empty stub function."
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        // [virtual] allows to override this method in descendant classes if it needs to add some additional logic when properties change.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Safely triggers the event if there are active subscribers (e.g., WPF UI elements).
            // It notifies all listeners that a property value has changed by passing the current 
            // object instance (this) and the name of the updated property (via PropertyChangedEventArgs)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}