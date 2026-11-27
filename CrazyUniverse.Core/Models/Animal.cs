using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CrazyUniverse.Core.Events;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Models
{
    // Declare an abstract base class representing any animal in the Crazy Zoo and implement INotifyPropertyChanged
    public abstract class Animal : INotifyPropertyChanged
    {
        // Shared default swimming distance in metres used by UI action labels and swimming commands
        public const double DefaultSwimDistance = 15.0;

        // Validated satiety delta applied when a neighbor reacts to a CrazyAction
        private const int CrazyNeighborReactionSatietyDelta = 2;

        // Allowed bounds for validating restored animal state
        public const int MaxNameLength = 100;
        public const int MaxDescriptionLength = 500;
        public const int MinSatiety = 0;
        public const int MaxSatiety = 100;

        // Read-only property for the unique identifier, automatically generated using Guid
        // example: "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        private int _satiety;       // Private variable to store the satiety level

        public string Name { get; } // Read-only property for the animal's name, set via constructor

        // Abstract property for the animal's species/type name (e.g., Monkey, Lion, Penguin)
        public abstract string TypeName { get; }

        // Abstract property for a funny description of the animal
        public abstract string Description { get; protected set; }

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

        // Protected constructor: receives initial name, validates it, and initializes the animal
        protected Animal(string name) : this(name, null)
        {
        }

        // Protected constructor: receives both name and optional description, validates name,
        // sets up the base state (max satiety & auto-generated ID) and custom description if provided
        protected Animal(string name, string? description)
        {
            // make sure the name is not null, empty, or just whitespace
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(Messages.Err_AnimalNameRequired, nameof(name));

            string trimmedName = name.Trim(); // Clean up any accidental spaces around the name and save it
            if (trimmedName.Length > MaxNameLength)
                throw new ArgumentException(string.Format(Messages.Err_NameTooLong, MaxNameLength), nameof(name));

            Name = trimmedName;
            Satiety = MaxSatiety; // Give every new animal a maximum satiety level of 100 right from the start

            if (!string.IsNullOrWhiteSpace(description))
            {
                string trimmedDescription = description.Trim();
                if (trimmedDescription.Length > MaxDescriptionLength)
                    throw new ArgumentException(string.Format(Messages.Err_DescriptionTooLong, MaxDescriptionLength), nameof(description));

                Description = trimmedDescription;
            }
        }

        // Protected restore constructor: validates and restores ID, name, optional description, and satiety from saved data
        protected Animal(string id, string name, string? description, int satiety)
        {
            // Validate that restored ID is a non-empty valid GUID
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id.Trim(), out _))
                throw new ArgumentException(Messages.Err_InvalidAnimalId, nameof(id));

            // Validate that restored name is present and within allowed length
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(Messages.Err_AnimalNameRequired, nameof(name));

            string trimmedName = name.Trim();
            if (trimmedName.Length > MaxNameLength)
                throw new ArgumentException(string.Format(Messages.Err_NameTooLong, MaxNameLength), nameof(name));

            // Validate that optional restored description does not exceed allowed length
            string? trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (trimmedDescription != null && trimmedDescription.Length > MaxDescriptionLength)
                throw new ArgumentException(string.Format(Messages.Err_DescriptionTooLong, MaxDescriptionLength), nameof(description));

            // Reject out-of-range satiety when restoring saved data instead of silently clamping it
            if (satiety < MinSatiety || satiety > MaxSatiety)
                throw new ArgumentOutOfRangeException(
                    nameof(satiety),
                    string.Format(Messages.Err_SatietyOutOfRange, satiety, MinSatiety, MaxSatiety));

            Id = id.Trim();
            Name = trimmedName;
            if (trimmedDescription != null)
            {
                Description = trimmedDescription;
            }
            _satiety = satiety;
        }

        // Restores persisted Id, optional Description, and validated Satiety state within Core without exposing public setters
        protected internal void RestoreState(string id, string? description, int satiety)
        {
            // Validate ID and satiety range before applying restored values
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id.Trim(), out _))
                throw new ArgumentException(Messages.Err_InvalidAnimalId, nameof(id));

            if (satiety < MinSatiety || satiety > MaxSatiety)
                throw new ArgumentOutOfRangeException(
                    nameof(satiety),
                    string.Format(Messages.Err_SatietyOutOfRange, satiety, MinSatiety, MaxSatiety));

            if (!string.IsNullOrWhiteSpace(description))
            {
                string trimmedDescription = description.Trim();
                if (trimmedDescription.Length > MaxDescriptionLength)
                    throw new ArgumentException(string.Format(Messages.Err_DescriptionTooLong, MaxDescriptionLength), nameof(description));

                Description = trimmedDescription;
            }

            Id = id.Trim();
            _satiety = satiety;
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Satiety));
        }

        // Virtual method for feeding: adds FeedCost points if the animal is not already fully stuffed
        public virtual string Feed()
        {
            // Check if the animal is already at maximum satiety (100)
            if (Satiety >= 100)
            {
                return string.Format(Messages.Action_FeedAlreadyFull, Name);
            }

            Satiety += FeedCost;   // Increase satiety by FeedCost points per feeding action

            // Return a descriptive message about the feeding action and current state
            return string.Format(Messages.Action_FeedSuccess, Name, Satiety);
        }

        // Abstract method: forces every child class (like Monkey or Lion) to create its own unique crazy action
        public abstract string CrazyAction();

        // ==========================================
        // DOMAIN EVENT REACTION LOGIC
        // ==========================================

        // Virtual method to react to events triggered by neighboring animals in the same repository/enclosure
        public virtual string ReactToNeighbor(Animal initiator, string eventType)
        {
            // Ignore events if the initiator is this exact same animal instance (self-reaction prevention)
            if (initiator == this || initiator.Id == this.Id)
            {
                return string.Empty;
            }

            // Handle reaction based on the domain event type
            switch (eventType)
            {
                case "Add":
                    // Format addition greeting message depending on animal type or fallback default
                    return this switch
                    {
                        Monkey => string.Format(Messages.Event_Add_Monkey, Name, initiator.Name),
                        Lion => string.Format(Messages.Event_Add_Lion, Name, initiator.Name),
                        Penguin => string.Format(Messages.Event_Add_Penguin, Name, initiator.Name),
                        HoneyBadger => string.Format(Messages.Event_Add_HoneyBadger, Name, initiator.Name),
                        _ => string.Format(Messages.Event_Add_Default, initiator.Name)
                    };

                case "Remove":
                    // Format removal farewell message depending on animal type or fallback default
                    return this switch
                    {
                        Monkey => string.Format(Messages.Event_Remove_Monkey, initiator.Name),
                        Lion => string.Format(Messages.Event_Remove_Lion, initiator.Name),
                        Penguin => string.Format(Messages.Event_Remove_Penguin, initiator.Name),
                        HoneyBadger => string.Format(Messages.Event_Remove_HoneyBadger, initiator.Name),
                        _ => string.Format(Messages.Event_Remove_Default, initiator.Name)
                    };

                case "CrazyAction":
                    // Neighboring animals react to a crazy action and lose a small validated amount of satiety (clamped at 0)
                    if (Satiety > 0)
                    {
                        Satiety -= CrazyNeighborReactionSatietyDelta;
                    }

                    string baseReaction = this switch
                    {
                        Monkey => string.Format(Messages.Event_Crazy_Monkey, initiator.Name),
                        Lion => string.Format(Messages.Event_Crazy_Lion, initiator.Name),
                        Penguin => string.Format(Messages.Event_Crazy_Penguin, initiator.Name),
                        HoneyBadger => string.Format(Messages.Event_Crazy_HoneyBadger, initiator.Name),
                        _ => string.Format(Messages.Event_Crazy_Default, initiator.Name)
                    };

                    return string.Format(Messages.Event_Crazy_NeighborSatiety, baseReaction, Name, Satiety);

                default:
                    return string.Empty;
            }
        }

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