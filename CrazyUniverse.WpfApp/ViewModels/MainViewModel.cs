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

        // Property representing sorting mode inside this specific enclosure (false = A-Z, true = Z-A)
        private bool _isSortedDescending;
        public bool IsSortedDescending
        {
            get => _isSortedDescending;
            set
            {
                if (_isSortedDescending != value)
                {
                    _isSortedDescending = value;
                    OnPropertyChanged(nameof(IsSortedDescending));
                }
            }
        }

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
        private readonly IRepository<Kangaroo> _kangarooRepository = new Repository<Kangaroo>();
        private readonly IRepository<Dolphin> _dolphinRepository = new Repository<Dolphin>();
        private readonly IRepository<Frog> _frogRepository = new Repository<Frog>();

        // Collection of enclosures for the accordion-based UI view
        public ObservableCollection<EnclosureViewModel> Enclosures { get; } = new ObservableCollection<EnclosureViewModel>();

        // ObservableCollection to store and track currently selected enclosure's animals or total view
        public ObservableCollection<Animal> Animals { get; } = new ObservableCollection<Animal>();

        // Search query string used for LINQ Where filtering across enclosures
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                    RefreshEnclosures(); // Refreshes and filters animal lists dynamically using LINQ Where
                }
            }
        }

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
        // PROPERTIES FOR UI COUNTERS & LOGS & LINQ GROUP BY
        // ==========================================

        // creates a collection of logs that is displayed in the user interface
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        // Общий подсчет животных во всех вольерах сразу
        public int ZooPopulation => _monkeyRepository.GetAll().Count()
                                    + _lionRepository.GetAll().Count()
                                    + _penguinRepository.GetAll().Count()
                                    + _honeyBadgerRepository.GetAll().Count()
                                    + _kangarooRepository.GetAll().Count()
                                    + _dolphinRepository.GetAll().Count()
                                    + _frogRepository.GetAll().Count();

        public int TotalAnimalsCount => ZooPopulation;

        // LINQ GroupBy Statistics: groups all zoo animals by Satiety ranges using resource keys (guaranteeing all 3 rows always appear)
        public IEnumerable<object> SatietyGroupsSummary
        {
            get
            {
                var allAnimals = _monkeyRepository.GetAll().Cast<Animal>()
                    .Concat(_lionRepository.GetAll().Cast<Animal>())
                    .Concat(_penguinRepository.GetAll().Cast<Animal>())
                    .Concat(_honeyBadgerRepository.GetAll().Cast<Animal>())
                    .Concat(_kangarooRepository.GetAll().Cast<Animal>())
                    .Concat(_dolphinRepository.GetAll().Cast<Animal>())
                    .Concat(_frogRepository.GetAll().Cast<Animal>())
                    .ToList();

                var predefinedCategories = new[]
                {
                    Messages.SatietyGroup_Low,
                    Messages.SatietyGroup_Medium,
                    Messages.SatietyGroup_High
                };

                var actualGroups = allAnimals
                    .GroupBy(a =>
                    {
                        if (a.Satiety <= 30) return Messages.SatietyGroup_Low;
                        if (a.Satiety <= 70) return Messages.SatietyGroup_Medium;
                        return Messages.SatietyGroup_High;
                    })
                    .ToDictionary(g => g.Key, g => g.Count());

                return predefinedCategories.Select(category => new
                {
                    GroupName = category,
                    Count = actualGroups.ContainsKey(category) ? actualGroups[category] : 0
                }).ToList();
            }
        }

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
        public ICommand ToggleSortCommand { get; } // Command to toggle sorting direction inside specific enclosure via LINQ OrderBy

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
                    Penguin p => p.SwimCost * 15,
                    Dolphin d => d.SwimCost * 15,
                    Frog f => f.SwimCost * 15,
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
                    Kangaroo k => k.JumpCost,
                    Frog f => f.JumpCost,
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
                    Kangaroo k => k.SoundCost,
                    Dolphin d => d.SoundCost,
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
            _kangarooRepository.LogMessageGenerated += (s, msg) => Log(msg);
            _dolphinRepository.LogMessageGenerated += (s, msg) => Log(msg);
            _frogRepository.LogMessageGenerated += (s, msg) => Log(msg);

            // Initialize enclosures collection based on available types from factory
            InitializeEnclosures();

            // Get initial animals from factory and populate respective repositories and enclosure view models
            var defaultAnimals = AnimalFactory.CreateDefaultAnimals();

            foreach (var animal in defaultAnimals)
            {
                if (animal is Monkey m)
                {
                    _monkeyRepository.Add(m);
                }
                else if (animal is Lion l)
                {
                    _lionRepository.Add(l);
                }
                else if (animal is Penguin p)
                {
                    _penguinRepository.Add(p);
                }
                else if (animal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.Add(hb);
                }
                else if (animal is Kangaroo k)
                {
                    _kangarooRepository.Add(k);
                }
                else if (animal is Dolphin d)
                {
                    _dolphinRepository.Add(d);
                }
                else if (animal is Frog f)
                {
                    _frogRepository.Add(f);
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
            ToggleSortCommand = new RelayCommand(ExecuteToggleSort);

            // Log initial application startup message
            Log(Messages.Log_ZooInit);
        }

        // ==========================================
        // ENCLOSURES INITIALIZATION HELPER
        // ==========================================
        private void InitializeEnclosures()
        {
            var types = AnimalFactory.GetAvailableTypes();

            foreach (var type in types)
            {
                var enclosure = new EnclosureViewModel
                {
                    Name = type,
                    DisplayName = type switch
                    {
                        "Lion" => Messages.UI_LionEnclosure,
                        "Penguin" => Messages.UI_PenguinEnclosure,
                        "HoneyBadger" => Messages.UI_HoneyBadgerEnclosure,
                        "Kangaroo" => Messages.UI_KangarooEnclosure,
                        "Dolphin" => Messages.UI_DolphinEnclosure,
                        "Frog" => Messages.UI_FrogEnclosure,
                        _ => Messages.UI_MonkeyEnclosure
                    }
                };

                // Безопасное получение списка животных для вольера при инициализации
                IEnumerable<Animal> repoItems = type switch
                {
                    "Lion" => _lionRepository.GetAll().Cast<Animal>(),
                    "Penguin" => _penguinRepository.GetAll().Cast<Animal>(),
                    "HoneyBadger" => _honeyBadgerRepository.GetAll().Cast<Animal>(),
                    "Kangaroo" => _kangarooRepository.GetAll().Cast<Animal>(),
                    "Dolphin" => _dolphinRepository.GetAll().Cast<Animal>(),
                    "Frog" => _frogRepository.GetAll().Cast<Animal>(),
                    _ => _monkeyRepository.GetAll().Cast<Animal>()
                };

                foreach (var item in repoItems.OrderBy(a => a.Name))
                {
                    enclosure.Animals.Add(item);
                }

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
        // REPOSITORY UPDATE & LINQ FILTERING/SORTING HELPER
        // ==========================================

        // Refreshes all enclosures applying LINQ Where (filtering) and sorting by name
        private void RefreshEnclosures()
        {
            foreach (var enc in Enclosures)
            {
                IEnumerable<Animal> repoItems = enc.Name switch
                {
                    "Lion" => _lionRepository.GetAll().Cast<Animal>(),
                    "Penguin" => _penguinRepository.GetAll().Cast<Animal>(),
                    "HoneyBadger" => _honeyBadgerRepository.GetAll().Cast<Animal>(),
                    "Kangaroo" => _kangarooRepository.GetAll().Cast<Animal>(),
                    "Dolphin" => _dolphinRepository.GetAll().Cast<Animal>(),
                    "Frog" => _frogRepository.GetAll().Cast<Animal>(),
                    _ => _monkeyRepository.GetAll().Cast<Animal>()
                };

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    repoItems = repoItems.Where(a =>
                        a.Name != null && a.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Сортировка исключительно по имени (A-Z или Z-A)
                var sortedItems = enc.IsSortedDescending
                    ? repoItems.OrderByDescending(a => a.Name).ToList()
                    : repoItems.OrderBy(a => a.Name).ToList();

                enc.Animals.Clear();
                foreach (var item in sortedItems)
                {
                    enc.Animals.Add(item);
                }
            }

            // Sync main Animals collection with currently selected repository type view
            Animals.Clear();
            var selectedEncItems = GetEnclosure(SelectedRepositoryType)?.Animals;
            if (selectedEncItems != null)
            {
                foreach (var item in selectedEncItems)
                {
                    Animals.Add(item);
                }
            }

            OnPropertyChanged(nameof(ZooPopulation));
            OnPropertyChanged(nameof(TotalAnimalsCount));
            OnPropertyChanged(nameof(SatietyGroupsSummary));
        }

        // shows the animals of the selected repository type in the UI
        private void UpdateDisplayedAnimals()
        {
            RefreshEnclosures();

            // Resets the currently selected animal to null
            // so that the old selected animal does not “hang” in memory as allocated to a list that does not exist on the screen.
            SelectedAnimal = null;
        }

        // ==========================================
        // COMMAND LOGIC METHODS
        // ==========================================

        // Command handler to toggle sorting direction inside an enclosure
        private void ExecuteToggleSort(object? parameter)
        {
            if (parameter is EnclosureViewModel enc)
            {
                // Toggle sorting direction (A-Z <-> Z-A)
                enc.IsSortedDescending = !enc.IsSortedDescending;

                // Перезапускаем обновление, чтобы RefreshEnclosures применил новый порядок имени
                RefreshEnclosures();
            }
        }

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
                }
                else if (newAnimal is Lion l)
                {
                    _lionRepository.Add(l);
                }
                else if (newAnimal is Penguin p)
                {
                    _penguinRepository.Add(p);
                }
                else if (newAnimal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.Add(hb);
                }
                else if (newAnimal is Kangaroo k)
                {
                    _kangarooRepository.Add(k);
                }
                else if (newAnimal is Dolphin d)
                {
                    _dolphinRepository.Add(d);
                }
                else if (newAnimal is Frog f)
                {
                    _frogRepository.Add(f);
                }

                UpdateDisplayedAnimals();

                // Add newly created animal to collection and set as selected
                SelectedAnimal = newAnimal;

                // Notify UI that counter properties have changed
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));
                OnPropertyChanged(nameof(SatietyGroupsSummary));

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
                }
                else if (SelectedAnimal is Lion l)
                {
                    _lionRepository.Remove(l);
                }
                else if (SelectedAnimal is Penguin p)
                {
                    _penguinRepository.Remove(p);
                }
                else if (SelectedAnimal is HoneyBadger hb)
                {
                    _honeyBadgerRepository.Remove(hb);
                }
                else if (SelectedAnimal is Kangaroo k)
                {
                    _kangarooRepository.Remove(k);
                }
                else if (SelectedAnimal is Dolphin d)
                {
                    _dolphinRepository.Remove(d);
                }
                else if (SelectedAnimal is Frog f)
                {
                    _frogRepository.Remove(f);
                }

                UpdateDisplayedAnimals();

                // After an animal is removed from the collection,
                // it should be “reset” so that SelectedAnimal does not refer to the deleted object to nowhere.
                SelectedAnimal = null;

                // Notify UI that counter properties have changed
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));
                OnPropertyChanged(nameof(SatietyGroupsSummary));

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
                OnPropertyChanged(nameof(SatietyGroupsSummary));

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
                OnPropertyChanged(nameof(SatietyGroupsSummary));
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
                OnPropertyChanged(nameof(SatietyGroupsSummary));
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
                OnPropertyChanged(nameof(SatietyGroupsSummary));
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
            if (SelectedNameCheck() && SelectedAnimal != null)
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
                else if (SelectedAnimal is Kangaroo k)
                {
                    _kangarooRepository.TriggerCrazyAction(k);
                }
                else if (SelectedAnimal is Dolphin d)
                {
                    _dolphinRepository.TriggerCrazyAction(d);
                }
                else if (SelectedAnimal is Frog f)
                {
                    _frogRepository.TriggerCrazyAction(f);
                }

                OnPropertyChanged(nameof(SelectedAnimal));
                OnPropertyChanged(nameof(SatietyGroupsSummary));

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

        private bool SelectedNameCheck() => SelectedAnimal != null;

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