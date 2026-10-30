using CrazyUniverse.Core.Resources;
using CrazyUniverse.WpfApp.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CrazyUniverse.WpfApp.ViewModels
{
    // ViewModel for the Add Animal dialog window implementing INotifyPropertyChanged
    public class AddAnimalViewModel : INotifyPropertyChanged
    {
        // ==========================================
        // UI DATA SOURCES & DICTIONARIES
        // ==========================================

        // Dictionary to map technical class names to localized display names for the ComboBox
        public Dictionary<string, string> AnimalOptions { get; } = new Dictionary<string, string>
        {
            { "Monkey", Messages.UI_MonkeyTypeName },
            { "Lion", Messages.UI_LionTypeName },
            { "Penguin", Messages.UI_PenguinTypeName },
            { "HoneyBadger", Messages.UI_HoneyBadgerTypeName },
            { "Kangaroo", Messages.UI_KangarooTypeName },
            { "Dolphin", Messages.UI_DolphinTypeName },
            { "Frog", Messages.UI_FrogTypeName }
        };

        // ==========================================
        // BACKING FIELDS & BINDABLE PROPERTIES
        // ==========================================

        private string _selectedAnimalType = "Monkey";

        // Public property for selected animal type with change notification
        public string SelectedAnimalType
        {
            get => _selectedAnimalType;
            set
            {
                if (_selectedAnimalType == value) return;
                _selectedAnimalType = value;
                OnPropertyChanged();
            }
        }

        private string _animalName = string.Empty;

        // Public property for animal name with change notification and Command re-evaluation
        public string AnimalName
        {
            get => _animalName;
            set
            {
                if (_animalName == value) return;
                _animalName = value;
                OnPropertyChanged();
                // Invalidate query suggested to update CanExecute state of AddCommand dynamically
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _animalDescription = string.Empty;

        // Public property for animal description with change notification and Command re-evaluation
        public string AnimalDescription
        {
            get => _animalDescription;
            set
            {
                if (_animalDescription == value) return;
                _animalDescription = value;
                OnPropertyChanged();
                // Invalidate query suggested to update CanExecute state of AddCommand dynamically
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string? _errorMessage;

        // Public property for displaying validation error messages in UI
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // ==========================================
        // EVENTS & COMMANDS FOR UI BINDING
        // ==========================================

        // Event to communicate dialog result back to code-behind
        public event System.Action<bool>? CloseRequested;

        // Command for adding a new animal
        public ICommand AddCommand { get; }

        // Command for canceling the dialog
        public ICommand CancelCommand { get; }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public AddAnimalViewModel()
        {
            AddCommand = new RelayCommand(ExecuteAdd, CanExecuteAdd);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        // ==========================================
        // COMMAND LOGIC & VALIDATION METHODS
        // ==========================================

        // Execution logic for add command with validation using resources
        private void ExecuteAdd(object? parameter)
        {
            // Validate required name field
            if (string.IsNullOrWhiteSpace(AnimalName))
            {
                ErrorMessage = Messages.Err_AnimalNameRequired;
                return;
            }

            // Validate required description field
            if (string.IsNullOrWhiteSpace(AnimalDescription))
            {
                ErrorMessage = Messages.Err_DescriptionRequired;
                return;
            }

            // Clear errors and request window closure with success result
            ErrorMessage = null;
            CloseRequested?.Invoke(true);
        }

        // Condition logic to check if add command can be executed based on input filling
        private bool CanExecuteAdd(object? parameter)
        {
            // Enable command only when both name and description are not empty or whitespace
            return !string.IsNullOrWhiteSpace(AnimalName) && !string.IsNullOrWhiteSpace(AnimalDescription);
        }

        // Execution logic for cancel command
        private void ExecuteCancel(object? parameter)
        {
            CloseRequested?.Invoke(false);
        }

        // ==========================================
        // NOTIFY PROPERTY CHANGED IMPLEMENTATION
        // ==========================================

        // Standard INotifyPropertyChanged implementation event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Helper method to raise PropertyChanged event automatically using caller member name
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}