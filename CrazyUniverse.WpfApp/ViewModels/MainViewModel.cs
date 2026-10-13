using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;
using CrazyUniverse.WpfApp.Commands;
using CrazyUniverse.WpfApp.Factories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CrazyUniverse.WpfApp.ViewModels
{
    // Implement INotifyPropertyChanged interface to notify UI when property values change
    public class MainViewModel : INotifyPropertyChanged
    {
        // ObservableCollection to store and track animals dynamically
        public ObservableCollection<Animal> Animals { get; set; }

        private Animal? _selectedAnimal;

        // Selected animal property with change notification
        public Animal? SelectedAnimal
        {
            get => _selectedAnimal;
            set
            {
                // If the new animal that was chosen is exactly the same as the old one, then do nothing and leave.
                if (_selectedAnimal == value) return;

                // Update selected animal with new value
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
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        public int ZooPopulation => Animals.Count;
        public int TotalAnimalsCount => Animals.Count;

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
            // Get initial animals from factory
            var defaultAnimals = AnimalFactory.CreateDefaultAnimals();
            Animals = new ObservableCollection<Animal>(defaultAnimals);

            // Initialize commands with actions and conditions
            AddAnimalCommand = new RelayCommand(ExecuteAddAnimal);
            RemoveAnimalCommand = new RelayCommand(ExecuteRemoveAnimal, CanExecuteRemoveAnimal);
            FeedCommand = new RelayCommand(ExecuteFeed, CanExecuteFeed);
            SwimCommand = new RelayCommand(ExecuteSwim, CanExecuteSwim);
            JumpCommand = new RelayCommand(ExecuteJump, CanExecuteJump);
            MakeSoundCommand = new RelayCommand(ExecuteMakeSound, CanExecuteMakeSound);
            CrazyActionCommand = new RelayCommand(ExecuteCrazyAction, CanExecuteCrazyAction);

            // Log initial application startup message
            Log(Messages.Log_ZooInit);
        }

        // ==========================================
        // COMMAND LOGIC METHODS
        // ==========================================

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

                // Add newly created animal to collection and set as selected
                Animals.Add(newAnimal);
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

                // Removes selected animal from Animals list
                // ObservableCollection notifies WPF to remove item from UI automatically
                Animals.Remove(SelectedAnimal);

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

        // Logic for crazy action
        private void ExecuteCrazyAction(object? parameter)
        {
            if (SelectedAnimal != null)
            {
                string resultMessage = SelectedAnimal.CrazyAction();
                OnPropertyChanged(nameof(SelectedAnimal));
                Log(resultMessage);

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