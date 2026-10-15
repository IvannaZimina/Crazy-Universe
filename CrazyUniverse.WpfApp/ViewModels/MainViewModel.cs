using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Repositories;
using CrazyUniverse.Core.Resources;
using CrazyUniverse.WpfApp.Commands;
using CrazyUniverse.WpfApp.Factories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CrazyUniverse.WpfApp.ViewModels
{
    // ==========================================
    // ENCLOSURE VIEW MODEL (Represents a single repository/enclosure in the accordion)
    // ==========================================
    public class EnclosureViewModel : INotifyPropertyChanged
    {
        // Name of the enclosure (e.g., "Monkey", "Lion")
        public string Name { get; set; } = string.Empty;

        // Display name localized or formatted for UI
        public string DisplayName { get; set; } = string.Empty;

        // Collection of animals living inside this specific enclosure
        public ObservableCollection<Animal> Animals { get; } = new ObservableCollection<Animal>();

        // Standard INotifyPropertyChanged implementation for enclosure updates
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ==========================================
    // MAIN VIEW MODEL
    // ==========================================

    // Implement INotifyPropertyChanged interface to notify UI when property values change
    public class MainViewModel : INotifyPropertyChanged
    {
        // ==========================================
        // TYPE-BASED REPOSITORIES & ENCLOSURES
        // ==========================================
        private readonly IRepository<Monkey> _monkeyRepository = new Repository<Monkey>();
        private readonly IRepository<Lion> _lionRepository = new Repository<Lion>();
        private readonly IRepository<Penguin> _penguinRepository = new Repository<Penguin>();
        private readonly IRepository<HoneyBadger> _honeyBadgerRepository = new Repository<HoneyBadger>();

        // Collection of enclosures for the accordion-based UI view
        public ObservableCollection<EnclosureViewModel> Enclosures { get; } = new ObservableCollection<EnclosureViewModel>();

        // ObservableCollection to store and track currently selected enclosure's animals or total view
        public ObservableCollection<Animal> Animals { get; } = new ObservableCollection<Animal>();

        private EnclosureViewModel? _selectedEnclosure;
        // Currently selected enclosure in the accordion
        public EnclosureViewModel? SelectedEnclosure
        {
            get => _selectedEnclosure;
            set
            {
                if (_selectedEnclosure == value) return;
                _selectedEnclosure = value;
                OnPropertyChanged(nameof(SelectedEnclosure));

                if (value != null)
                {
                    SelectedRepositoryType = value.Name;
                }
            }
        }

        // currently selected repository type, defaulting to "Monkey" for initial view
        private string _selectedRepositoryType = "Monkey";

        // Selected repository type property with change notification
        public string SelectedRepositoryType
        {
            get => _selectedRepositoryType;
            set
            {
                // If the selected type is the same as before, do nothing
                if (_selectedRepositoryType == value) return;

                // Update the field with the new repository type
                _selectedRepositoryType = value;

                // Notify UI that the selected repository type has changed
                OnPropertyChanged(nameof(SelectedRepositoryType));

                // Refresh the list of displayed animals to match the new repository type
                UpdateDisplayedAnimals();
            }
        }

        private Animal? _selectedAnimal;

        // Selected animal property with change notification
        public Animal? SelectedAnimal
        {
            get => _selectedAnimal;
            set
            {
                // Removed early return on equality to allow re-triggering selection across multi-list accordions
                _selectedAnimal = value;

                // Call OnPropertyChanged method to inform View that selected animal has changed
                OnPropertyChanged(nameof(SelectedAnimal));
                OnPropertyChanged(nameof(CanSelectedAnimalSwim));
                OnPropertyChanged(nameof(CanSelectedAnimalJump));
                OnPropertyChanged(nameof(CanSelectedAnimalMakeSound));
                OnPropertyChanged(nameof(SwimButtonText));
                OnPropertyChanged(nameof(JumpButtonText));
                OnPropertyChanged(nameof(MakeSoundButtonText));
            }
        }

        // ==========================================
        // PROPERTIES FOR UI COUNTERS & LOGS
        // ==========================================

        // creates a collection of logs that is displayed in the user interface
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        // Общий подсчет животных во всех вольерах сразу
        public int ZooPopulation => _monkeyRepository.GetAll().Count()
                                  + _lionRepository.GetAll().Count()
                                  + _penguinRepository.GetAll().Count()
                                  + _honeyBadgerRepository.GetAll().Count();

        public int TotalAnimalsCount => ZooPopulation;

        // ==========================================
        // COMMANDS FOR UI BINDING
        // ==========================================
        public ICommand AddAnimalCommand { get; }
        public ICommand RemoveAnimalCommand { get; }
        public ICommand FeedCommand { get; }
        public ICommand SwimCommand { get; }
        public ICommand JumpCommand { get; }
        public ICommand MakeSoundCommand { get; }
        public ICommand CrazyActionCommand { get; }
        public ICommand SelectAnimalCommand { get; } // Command to handle robust animal selection across multi-list accordions

        // ==========================================
        // PROPERTIES FOR SELECTED ANIMAL ABILITIES
        // ==========================================
        public bool CanSelectedAnimalSwim => SelectedAnimal is ISwimmable;
        public bool CanSelectedAnimalJump => SelectedAnimal is IJumpable;
        public bool CanSelectedAnimalMakeSound => SelectedAnimal is IVocalizable;

        // ==========================================
        // PROPERTIES FOR DYNAMIC BUTTON TEXTS
        // ==========================================
        public string SwimButtonText
        {
            get
            {
                int cost = SelectedAnimal switch
                {
                    HoneyBadger hb => hb.SwimCost,
                    Penguin p => p.SwimCost,
                    _ => 10
                };
                return string.Format(Messages.UI_BtnSwim, cost);
            }
        }

        public string JumpButtonText
        {
            get
            {
                int cost = SelectedAnimal switch
                {
                    Monkey m => m.JumpCost,
                    _ => 5
                };
                return string.Format(Messages.UI_BtnJump, cost);
            }
        }

        public string MakeSoundButtonText
        {
            get
            {
                int cost = SelectedAnimal switch
                {
                    HoneyBadger hb => hb.SoundCost,
                    Lion l => 2,
                    Monkey m => 2,
                    Penguin p => 2,
                    _ => 2
                };
                return string.Format(Messages.UI_BtnMakeSound, cost);
            }
        }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public MainViewModel()
        {
            // Subscribe repositories to automatically route domain log messages to UI log collector
            _monkeyRepository.LogMessageGenerated += (s, msg) => Log(msg);
            _lionRepository.LogMessageGenerated += (s, msg) => Log(msg);
            _penguinRepository.LogMessageGenerated += (s, msg) => Log(msg);
            _honeyBadgerRepository.LogMessageGenerated += (s, msg) => Log(msg);

            // Initialize enclosures collection based on available types from factory
            InitializeEnclosures();

            // Get initial animals from factory and populate respective repositories and enclosure view models
            var defaultAnimals = AnimalFactory.CreateDefaultAnimals();

            foreach (var animal in defaultAnimals)
            {
                if (animal is Monkey m)
                {
                    _monkeyRepository.Add(m);
                    GetEnclosure("Monkey")?.Animals.Add(m);
                }
                else if (animal is Lion l)
                {
                    _lionRepository.Add(l);
                    GetEnclosure("Lion")?.Animals.Add(l);
                }
                else if (animal is Penguin p)
                {
                    _penguinRepository.Add(p);
                    GetEnclosure("Penguin")?.Animals.Add(p);
                }
                else if (animal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.Add(hb);
                    GetEnclosure("HoneyBadger")?.Animals.Add(hb);
                }
            }

            // Load initial view for default selected repository type ("Monkey")
            UpdateDisplayedAnimals();

            // Initialize commands with actions and conditions
            AddAnimalCommand = new RelayCommand(ExecuteAddAnimal);
            RemoveAnimalCommand = new RelayCommand(ExecuteRemoveAnimal, CanExecuteRemoveAnimal);
            FeedCommand = new RelayCommand(ExecuteFeed, CanExecuteFeed);
            SwimCommand = new RelayCommand(ExecuteSwim, CanExecuteSwim);
            JumpCommand = new RelayCommand(ExecuteJump, CanExecuteJump);
            MakeSoundCommand = new RelayCommand(ExecuteMakeSound, CanExecuteMakeSound);
            CrazyActionCommand = new RelayCommand(ExecuteCrazyAction, CanExecuteCrazyAction);
            SelectAnimalCommand = new RelayCommand(ExecuteSelectAnimal);

            // Log initial application startup message
            Log(Messages.Log_ZooInit);
        }

        // ==========================================
        // ENCLOSURES INITIALIZATION HELPER
        // ==========================================
        private void InitializeEnclosures()
        {
            // Fetch all available animal types dynamically from the factory (e.g., "Monkey", "Lion", etc.)
            var types = AnimalFactory.GetAvailableTypes();

            // Loop through each type to create a corresponding enclosure view model
            foreach (var type in types)
            {
                // Create a new enclosure container for the accordion UI
                var enclosure = new EnclosureViewModel
                {
                    // Set the technical identifier key used by repositories and logic
                    Name = type,

                    // Map the technical key to a localized plural enclosure name from resources (e.g., "Lions", "Monkeys")
                    DisplayName = type switch
                    {
                        "Lion" => Messages.UI_LionEnclosure,
                        "Penguin" => Messages.UI_PenguinEnclosure,
                        "HoneyBadger" => Messages.UI_HoneyBadgerEnclosure,
                        _ => Messages.UI_MonkeyEnclosure // Default fallback to Monkeys
                    }
                };

                // Add the configured enclosure to the main collection displayed in the accordion UI
                Enclosures.Add(enclosure);
            }
        }

        // Helper method to find an enclosure by its name key
        private EnclosureViewModel? GetEnclosure(string name)
        {
            foreach (var enc in Enclosures)
            {
                if (enc.Name == name) return enc;
            }
            return null;
        }

        // ==========================================
        // REPOSITORY UPDATE HELPER
        // ==========================================

        // shows the animals of the selected repository type in the UI
        private void UpdateDisplayedAnimals()
        {
            // Clears the current on-screen list of animals to remove old items from the previous category.
            Animals.Clear();

            // what exactly is currently selected in the interface drop-down list or accordion
            IEnumerable<Animal> items = SelectedRepositoryType switch
            {
                "Lion" => _lionRepository.GetAll(),
                "Penguin" => _penguinRepository.GetAll(),
                "HoneyBadger" => _honeyBadgerRepository.GetAll(),
                _ => _monkeyRepository.GetAll()
            };

            // Синхронизируем списки по всем репозиториям для каждого вольера, чтобы вольери всегда хранили актуальные коллекции
            foreach (var enc in Enclosures)
            {
                enc.Animals.Clear();

                IEnumerable<Animal> repoItems = enc.Name switch
                {
                    "Lion" => _lionRepository.GetAll(),
                    "Penguin" => _penguinRepository.GetAll(),
                    "HoneyBadger" => _honeyBadgerRepository.GetAll(),
                    _ => _monkeyRepository.GetAll()
                };

                foreach (var item in repoItems)
                {
                    enc.Animals.Add(item);
                }
            }

            foreach (var item in items)
            {
                Animals.Add(item);
            }

            // Resets the currently selected animal to null
            // so that the old selected animal does not “hang” in memory as allocated to a list that does not exist on the screen.
            SelectedAnimal = null;

            OnPropertyChanged(nameof(ZooPopulation));
            OnPropertyChanged(nameof(TotalAnimalsCount));
        }

        // ==========================================
        // COMMAND LOGIC METHODS
        // ==========================================

        // Logic to handle animal selection reliably across multiple enclosure lists
        private void ExecuteSelectAnimal(object? parameter)
        {
            if (parameter is Animal animal)
            {
                // Force deselect then select to ensure UI notification fires even on re-selecting the same item
                if (_selectedAnimal == animal)
                {
                    SelectedAnimal = null;
                }
                SelectedAnimal = animal;
            }
        }

        // Logic for opening dialog and adding a new animal
        private void ExecuteAddAnimal(object? parameter)
        {
            var dialog = new AddAnimal
            {
                // Set the current active Application window as Owner for proper centering
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Show modal dialog and check if user clicked "Add" (DialogResult = true)
            if (dialog.ShowDialog() == true)
            {
                // Request factory to create new animal using dialog data
                var newAnimal = AnimalFactory.CreateAnimal(
                    dialog.AnimalType,
                    dialog.AnimalName,
                    dialog.AnimalDescription
                );

                // Add animal to the corresponding type repository and its enclosure (triggers welcome domain events)
                if (newAnimal is Monkey m)
                {
                    _monkeyRepository.Add(m);
                    GetEnclosure("Monkey")?.Animals.Add(m);
                }
                else if (newAnimal is Lion l)
                {
                    _lionRepository.Add(l);
                    GetEnclosure("Lion")?.Animals.Add(l);
                }
                else if (newAnimal is Penguin p)
                {
                    _penguinRepository.Add(p);
                    GetEnclosure("Penguin")?.Animals.Add(p);
                }
                else if (newAnimal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.Add(hb);
                    GetEnclosure("HoneyBadger")?.Animals.Add(hb);
                }

                UpdateDisplayedAnimals();

                // Add newly created animal to collection and set as selected
                SelectedAnimal = newAnimal;

                // Notify UI that counter properties have changed
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));

                // Log the addition action
                Log(string.Format(Messages.Log_AddedAnimal, newAnimal.Name));
            }
        }

        // Logic for removing selected animal
        private void ExecuteRemoveAnimal(object? parameter)
        {
            if (SelectedAnimal != null)
            {
                string name = SelectedAnimal.Name;

                // Remove from appropriate type repository and its enclosure collection (triggers farewell domain events)
                if (SelectedAnimal is Monkey m)
                {
                    _monkeyRepository.Remove(m);
                    GetEnclosure("Monkey")?.Animals.Remove(m);
                }
                else if (SelectedAnimal is Lion l)
                {
                    _lionRepository.Remove(l);
                    GetEnclosure("Lion")?.Animals.Remove(l);
                }
                else if (SelectedAnimal is Penguin p)
                {
                    _penguinRepository.Remove(p);
                    GetEnclosure("Penguin")?.Animals.Remove(p);
                }
                else if (SelectedAnimal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.Remove(hb);
                    GetEnclosure("HoneyBadger")?.Animals.Remove(hb);
                }

                UpdateDisplayedAnimals();

                // After an animal is removed from the collection,
                // it should be “reset” so that SelectedAnimal does not refer to the deleted object to nowhere.
                SelectedAnimal = null;

                // Notify UI that counter properties have changed
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));

                // Log the removal action
                Log(string.Format(Messages.Log_RemovedAnimal, name));
            }
            else
            {
                // Log warning if no animal was selected
                Log(Messages.Log_NoAnimalRemove);
            }
        }

        // Condition: can remove only if an animal is selected
        private bool CanExecuteRemoveAnimal(object? parameter)
        {
            return SelectedAnimal != null;
        }

        // Logic for feeding the selected animal
        private void ExecuteFeed(object? parameter)
        {
            if (SelectedAnimal != null)
            {
                // Calls built-in Feed method of Animal core logic to increase satiety
                SelectedAnimal.Feed();

                // Forces UI to update progress bars or values if needed
                OnPropertyChanged(nameof(SelectedAnimal));

                // Refresh commands execution status so action buttons become active again
                CommandManager.InvalidateRequerySuggested();

                // Log the feeding action
                Log(string.Format(Messages.Action_AnimalFed, SelectedAnimal.Name, SelectedAnimal.Satiety));
            }
            else
            {
                // Log warning if no animal was selected
                Log(Messages.Log_NoAnimalFeed);
            }
        }

        // Condition: can feed only if an animal is selected
        private bool CanExecuteFeed(object? parameter)
        {
            return SelectedAnimal != null;
        }

        // Logic for swimming action
        private void ExecuteSwim(object? parameter)
        {
            if (SelectedAnimal is ISwimmable swimmable)
            {
                string resultMessage = swimmable.Swim(15);
                OnPropertyChanged(nameof(SelectedAnimal));
                Log(resultMessage);

                if (SelectedAnimal?.Satiety == 0)
                {
                    Log(string.Format(Messages.Action_NoEnergy, SelectedAnimal.Name));
                }
            }
        }

        // Condition: can swim only if selected animal implements ISwimmable
        private bool CanExecuteSwim(object? parameter)
        {
            return SelectedAnimal is ISwimmable && SelectedAnimal.Satiety > 0;
        }

        // Logic for jumping action
        private void ExecuteJump(object? parameter)
        {
            if (SelectedAnimal is IJumpable jumpable)
            {
                string resultMessage = jumpable.Jump();
                OnPropertyChanged(nameof(SelectedAnimal));
                Log(resultMessage);

                if (SelectedAnimal?.Satiety == 0)
                {
                    Log(string.Format(Messages.Action_NoEnergy, SelectedAnimal.Name));
                }
            }
        }

        // Condition: can jump only if selected animal implements IJumpable
        private bool CanExecuteJump(object? parameter)
        {
            return SelectedAnimal is IJumpable && SelectedAnimal.Satiety > 0;
        }

        // Logic for making sound action
        private void ExecuteMakeSound(object? parameter)
        {
            if (SelectedAnimal is IVocalizable vocalizable)
            {
                string resultMessage = vocalizable.MakeSound();
                OnPropertyChanged(nameof(SelectedAnimal));
                Log(resultMessage);

                if (SelectedAnimal?.Satiety == 0)
                {
                    Log(string.Format(Messages.Action_NoEnergy, SelectedAnimal.Name));
                }
            }
        }

        // Condition: can make sound only if selected animal implements IVocalizable
        private bool CanExecuteMakeSound(object? parameter)
        {
            return SelectedAnimal is IVocalizable && SelectedAnimal.Satiety > 0;
        }

        // Logic for crazy action (triggers domain events via repository)
        private void ExecuteCrazyAction(object? parameter)
        {
            if (SelectedAnimal != null)
            {
                // Trigger crazy action through the appropriate repository to broadcast reactions to enclosure neighbors
                if (SelectedAnimal is Monkey m)
                {
                    _monkeyRepository.TriggerCrazyAction(m);
                }
                else if (SelectedAnimal is Lion l)
                {
                    _lionRepository.TriggerCrazyAction(l);
                }
                else if (SelectedAnimal is Penguin p)
                {
                    _penguinRepository.TriggerCrazyAction(p);
                }
                else if (SelectedAnimal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.TriggerCrazyAction(hb);
                }

                OnPropertyChanged(nameof(SelectedAnimal));

                if (SelectedAnimal.Satiety == 0)
                {
                    Log(string.Format(Messages.Action_NoEnergy, SelectedAnimal.Name));
                }
            }
            else
            {
                Log(Messages.Log_NoAnimalCrazy);
            }
        }

        // Condition: can perform crazy action only if an animal is selected
        private bool CanExecuteCrazyAction(object? parameter)
        {
            return SelectedAnimal != null && SelectedAnimal.Satiety > 0;
        }

        // ==========================================
        // HELPER LOG METHOD
        // ==========================================
        private void Log(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            LogMessages.Add(timestampedMessage);
        }

        // ==========================================
        // NOTIFY PROPERTY CHANGED IMPLEMENTATION
        // ==========================================

        // Standard INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        // [CallerMemberName] - Automatically gets property name if not provided
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            // OnPropertyChanged - Sends signal to WPF that property changed so UI updates automatically
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}