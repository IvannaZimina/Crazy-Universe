using CrazyUniverse.Core.Events;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Resources;
using CrazyUniverse.Core.Storage;
using CrazyUniverse.Core.World;
using CrazyUniverse.WpfApp.Commands;
using CrazyUniverse.WpfApp.Factories;
using CrazyUniverse.WpfApp.Services;
using CrazyUniverse.WpfApp.Views; // Added views namespace for simulation window
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
// Uses generic DbException so ViewModel stays independent of SqlClient and works with EfCore too.
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        // WORLD, PERSISTENCE & ENCLOSURES
        // ==========================================
        // Domain world owned in Core, encapsulating all type-based repositories and routing
        private readonly ZooWorld _world;
        private readonly IWorldStorage? _worldStorage;
        private readonly WorldMapper _worldMapper;
        private readonly IFileDialogService? _fileDialogService;
        private readonly string _storageProvider;
        // Decouples ViewModel from hardcoded database provider names (OCP/DIP).
        private readonly bool _isFileBasedStorage;

        private bool _isBusy;
        // Indicates whether an async save or load operation is currently running
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

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
                OnPropertyChanged(nameof(FeedButtonText));
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

        public int ZooPopulation => _world.GetAllAnimals().Count;

        public int TotalAnimalsCount => ZooPopulation;

        public string ZooPopulationText => string.Format(Messages.UI_ZooPopulation, ZooPopulation);

        public string TotalAnimalsText => string.Format(Messages.UI_TotalAnimals, TotalAnimalsCount);

        public string EnclosureNotFoundText => Messages.UI_Enclosure_NotFound;

        // LINQ GroupBy Statistics: groups all zoo animals by Satiety ranges using resource keys (guaranteeing all 3 rows always appear)
        public IEnumerable<object> SatietyGroupsSummary
        {
            get
            {
                var allAnimals = _world.GetAllAnimals().ToList();

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
        public ICommand OpenSimulationCommand { get; } // Command to open the heavy CPU simulation dialog window
        public ICommand SaveWorldCommand { get; } // Command to save the world asynchronously
        public ICommand LoadWorldCommand { get; } // Command to load and restore the world asynchronously

        // ==========================================
        // PROPERTIES FOR SELECTED ANIMAL ABILITIES
        // ==========================================
        public bool CanSelectedAnimalSwim => SelectedAnimal is ISwimmable;
        public bool CanSelectedAnimalJump => SelectedAnimal is IJumpable;
        public bool CanSelectedAnimalMakeSound => SelectedAnimal is IVocalizable;

        // ==========================================
        // PROPERTIES FOR DYNAMIC BUTTON TEXTS
        // ==========================================
        // Reads feed gain from Animal.FeedCost in Core
        public string FeedButtonText
        {
            get
            {
                int gain = SelectedAnimal?.FeedCost ?? 0;
                return string.Format(Messages.UI_BtnFeed, gain);
            }
        }

        // Reads swim cost through ISwimmable and shared default distance from Core without type-switch blocks
        public string SwimButtonText
        {
            get
            {
                int cost = SelectedAnimal is ISwimmable swimmable
                    ? (int)(swimmable.SwimCost * Animal.DefaultSwimDistance)
                    : 0;
                return string.Format(Messages.UI_BtnSwim, cost);
            }
        }

        // Reads jump cost directly from IJumpable without type-switch blocks
        public string JumpButtonText
        {
            get
            {
                int cost = SelectedAnimal is IJumpable jumpable
                    ? jumpable.JumpCost
                    : 0;
                return string.Format(Messages.UI_BtnJump, cost);
            }
        }

        // Reads sound cost directly from IVocalizable without type-switch blocks
        public string MakeSoundButtonText
        {
            get
            {
                int cost = SelectedAnimal is IVocalizable vocalizable
                    ? vocalizable.SoundCost
                    : 0;
                return string.Format(Messages.UI_BtnMakeSound, cost);
            }
        }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public MainViewModel() : this(new ZooWorld())
        {
        }

        // Accepts ZooWorld so repositories and type routing live in Core rather than inside ViewModel
        public MainViewModel(ZooWorld world)
            : this(world, null, new WorldMapper(), null, "Json", isFileBasedStorage: true)
        {
        }

        // DI constructor receiving storage, mapper, file dialog service, and configuration values
        public MainViewModel(
            ZooWorld world,
            IWorldStorage? worldStorage,
            WorldMapper? worldMapper,
            IFileDialogService? fileDialogService,
            string storageProvider,
            bool isFileBasedStorage = true)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _worldStorage = worldStorage;
            _worldMapper = worldMapper ?? new WorldMapper();
            _fileDialogService = fileDialogService;
            _storageProvider = string.IsNullOrWhiteSpace(storageProvider) ? "Json" : storageProvider.Trim();
            _isFileBasedStorage = isFileBasedStorage;

            _world.LogMessageGenerated += (s, e) => HandleRepositoryLog(e);

            // Initialize enclosures collection based on available types from factory
            InitializeEnclosures();

            // Get initial animals from factory and populate the Core world without initial log noise
            var defaultAnimals = AnimalFactory.CreateDefaultAnimals();
            foreach (var animal in defaultAnimals)
            {
                _world.Add(animal, suppressLog: true);
            }

            // Load initial view for default selected repository type ("Monkey")
            UpdateDisplayedAnimals();

            // Initialize commands with actions and conditions
            AddAnimalCommand = new RelayCommand(ExecuteAddAnimal, _ => !IsBusy);
            RemoveAnimalCommand = new RelayCommand(ExecuteRemoveAnimal, CanExecuteRemoveAnimal);
            FeedCommand = new RelayCommand(ExecuteFeed, CanExecuteFeed);
            SwimCommand = new RelayCommand(ExecuteSwim, CanExecuteSwim);
            JumpCommand = new RelayCommand(ExecuteJump, CanExecuteJump);
            MakeSoundCommand = new RelayCommand(ExecuteMakeSound, CanExecuteMakeSound);
            CrazyActionCommand = new RelayCommand(ExecuteCrazyAction, CanExecuteCrazyAction);
            SelectAnimalCommand = new RelayCommand(ExecuteSelectAnimal);
            ToggleSortCommand = new RelayCommand(ExecuteToggleSort, _ => !IsBusy);

            // Initialize simulation window opening command
            OpenSimulationCommand = new RelayCommand(ExecuteOpenSimulation, _ => !IsBusy);

            // Initialize async save and load commands
            SaveWorldCommand = new AsyncRelayCommand(_ => ExecuteSaveWorldAsync(CancellationToken.None), _ => CanExecutePersistenceCommand());
            LoadWorldCommand = new AsyncRelayCommand(_ => ExecuteLoadWorldAsync(CancellationToken.None), _ => CanExecutePersistenceCommand());

            // Log initial application startup message
            Log(Messages.Log_ZooInit);
        }

        private bool CanExecutePersistenceCommand() => !IsBusy && _worldStorage != null;

        // Saves the current world snapshot and writes result/error/cancellation to Event Log
        private async Task ExecuteSaveWorldAsync(CancellationToken token)
        {
            if (_worldStorage == null)
                return;

            string targetPath = string.Empty;
            // Prompts for a file path only when using a file-based storage provider.
            if (_isFileBasedStorage)
            {
                string? chosenPath = _fileDialogService?.ShowSaveFileDialog(_storageProvider);
                if (string.IsNullOrWhiteSpace(chosenPath))
                {
                    // Log when the user cancels the save dialog so the action is visible in Event Log
                    Log(Messages.Log_OperationCancelled);
                    return;
                }
                targetPath = chosenPath;
            }

            IsBusy = true;
            try
            {
                WorldDto dto = _worldMapper.ToDto(_world);
                await _worldStorage.SaveAsync(dto, targetPath, token);

                string locationInfo = _isFileBasedStorage ? $"{_storageProvider}: {targetPath}" : _storageProvider;
                Log(string.Format(Messages.Log_WorldSaved, locationInfo));
            }
            catch (OperationCanceledException)
            {
                Log(Messages.Log_OperationCancelled);
            }
            catch (Exception ex) when (
                ex is InvalidWorldDataException ||
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException ||
                ex is InvalidOperationException ||
                // Catches ADO.NET and EF Core database errors through a common base type.
                ex is DbException)
            {
                Log(string.Format(Messages.Log_SaveFailed, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Loads a world snapshot, validates/restores it atomically, and logs the outcome
        private async Task ExecuteLoadWorldAsync(CancellationToken token)
        {
            if (_worldStorage == null)
                return;

            string targetPath = string.Empty;
            // Prompts for a file path only when using a file-based storage provider.
            if (_isFileBasedStorage)
            {
                string? chosenPath = _fileDialogService?.ShowOpenFileDialog(_storageProvider);
                if (string.IsNullOrWhiteSpace(chosenPath))
                {
                    // Log when the user cancels the open dialog so the action is visible in Event Log
                    Log(Messages.Log_OperationCancelled);
                    return;
                }
                targetPath = chosenPath;
            }

            IsBusy = true;
            try
            {
                WorldDto dto = await _worldStorage.LoadAsync(targetPath, token);
                IReadOnlyList<Animal> restoredAnimals = _worldMapper.ToAnimals(dto);

                // Replace animals silently and atomically; if anything throws, existing world remains unchanged
                _world.Restore(restoredAnimals);

                UpdateDisplayedAnimals();
                SelectedAnimal = null;
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));
                OnPropertyChanged(nameof(ZooPopulationText));
                OnPropertyChanged(nameof(TotalAnimalsText));
                OnPropertyChanged(nameof(SatietyGroupsSummary));

                string locationInfo = _isFileBasedStorage ? $"{_storageProvider}: {targetPath}" : _storageProvider;
                Log(string.Format(Messages.Log_WorldLoaded, locationInfo));
            }
            catch (OperationCanceledException)
            {
                Log(Messages.Log_OperationCancelled);
            }
            catch (Exception ex) when (
                ex is InvalidWorldDataException ||
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException ||
                ex is InvalidOperationException ||
                // Catches ADO.NET and EF Core database errors through a common base type.
                ex is DbException)
            {
                Log(string.Format(Messages.Log_LoadFailed, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==========================================
        // ENCLOSURES INITIALIZATION HELPER
        // ==========================================
        private void InitializeEnclosures()
        {
            var types = AnimalFactory.GetAvailableTypes();

            foreach (var type in types)
            {
                var sampleAnimal = AnimalCatalog.CreateAnimal(type, "_probe");
                var enclosure = new EnclosureViewModel
                {
                    Name = type,
                    DisplayName = sampleAnimal.TypeName
                };

                foreach (var item in _world.GetAnimalsByType(type).OrderBy(a => a.Name))
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
                IEnumerable<Animal> repoItems = _world.GetAnimalsByType(enc.Name);

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
            OnPropertyChanged(nameof(ZooPopulationText));
            OnPropertyChanged(nameof(TotalAnimalsText));
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

        // Handler to open the simulation window as a modal dialog
        private void ExecuteOpenSimulation(object? parameter)
        {
            // Collect all current animals from the Core world
            var allAnimals = _world.GetAllAnimals().ToList();

            var simulationWindow = new SimulationWindow(allAnimals)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            simulationWindow.ShowDialog();
        }

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

                // Add animal through Core ZooWorld (triggers welcome domain events)
                _world.Add(newAnimal);

                UpdateDisplayedAnimals();

                // Add newly created animal to collection and set as selected
                SelectedAnimal = newAnimal;

                // Notify UI that counter properties have changed
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));
                OnPropertyChanged(nameof(ZooPopulationText));
                OnPropertyChanged(nameof(TotalAnimalsText));
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

                // Remove through Core ZooWorld (triggers farewell domain events)
                _world.Remove(SelectedAnimal);

                UpdateDisplayedAnimals();

                // After an animal is removed from the collection,
                // it should be “reset” so that SelectedAnimal does not refer to the deleted object to nowhere.
                SelectedAnimal = null;

                // Notify UI that counter properties have changed
                OnPropertyChanged(nameof(ZooPopulation));
                OnPropertyChanged(nameof(TotalAnimalsCount));
                OnPropertyChanged(nameof(ZooPopulationText));
                OnPropertyChanged(nameof(TotalAnimalsText));
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
            return !IsBusy && SelectedAnimal != null;
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
            return !IsBusy && SelectedAnimal != null;
        }

        // Logic for swimming action using the shared default distance from Core
        private void ExecuteSwim(object? parameter)
        {
            if (SelectedAnimal is ISwimmable swimmable)
            {
                string resultMessage = swimmable.Swim(Animal.DefaultSwimDistance);
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
            return !IsBusy && SelectedAnimal is ISwimmable && SelectedAnimal.Satiety > 0;
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
            return !IsBusy && SelectedAnimal is IJumpable && SelectedAnimal.Satiety > 0;
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
            return !IsBusy && SelectedAnimal is IVocalizable && SelectedAnimal.Satiety > 0;
        }

        // Logic for crazy action (triggers domain events via Core world)
        private void ExecuteCrazyAction(object? parameter)
        {
            // Capture animal reference and Id before RefreshEnclosures rebuilds observable collections
            var actingAnimal = SelectedAnimal;
            if (actingAnimal != null)
            {
                string actingAnimalId = actingAnimal.Id;

                // Trigger crazy action through Core ZooWorld to broadcast reactions to enclosure neighbors
                _world.TriggerCrazyAction(actingAnimal);

                // Refresh displayed lists and group summary because neighbor reactions can modify Satiety
                RefreshEnclosures();

                // Restore SelectedAnimal after collection refresh so UI controls stay enabled and bound to the animal
                var restoredAnimal = _world.GetAllAnimals().FirstOrDefault(a => a.Id == actingAnimalId) ?? actingAnimal;
                SelectedAnimal = restoredAnimal;
                OnPropertyChanged(nameof(SatietyGroupsSummary));
                CommandManager.InvalidateRequerySuggested();

                // Check energy on the captured animal instance to avoid NullReferenceException if selection changed
                if (actingAnimal.Satiety == 0)
                {
                    Log(string.Format(Messages.Action_NoEnergy, actingAnimal.Name));
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
            return !IsBusy && SelectedAnimal != null && SelectedAnimal.Satiety > 0;
        }

        // ==========================================
        // HELPER LOG METHODS
        // ==========================================

        // Handles typed repository log events containing Kind and Cause details
        private void HandleRepositoryLog(LogEventArgs e)
        {
            // You can use e.Kind and e.Cause here if needed for extended logging or filtering,
            // for now we extract e.Message to display in the UI log list.
            Log(e.Message);
        }

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