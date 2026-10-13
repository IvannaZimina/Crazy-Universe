using System;
using System.Reflection.Metadata;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrazyUniverse.WpfApp.Commands
{
    // Define public class implementing ICommand interface for WPF command binding
    public class RelayCommand : ICommand
    {
        // An action is a special type of delegate that stores a reference to a method (a piece of code) that returns nothing (void).
        //<object?> is a method that can accept a single input parameter of any reference type(or null).
        // _execute - variable that holds the action to be executed when the command is invoked.
        private readonly Action<object?> _execute;

        // Func<object?, bool> is a delegate that represents a method that takes an object? parameter and returns a boolean value.
        //(object?) is an input parameter(optional data to check).
        // for example, if button is enabled or disabled based on some condition
        private readonly Func<object?, bool>? _canExecute;

        // Initialize command with execution logic and optional condition
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            // Throw exception if execution action is null
            // ?? (null - coalescing operator) -If the left side(execute) is not null, then write it down.
            // And if it is null, then execute what's written on the right."
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));

            // Assign condition function
            _canExecute = canExecute;
        }

        // Required by ICommand interface. Notifies WPF to recheck button availability when application state changes.
        public event EventHandler? CanExecuteChanged
        {
            // custom event accessors:

            // Add event handler to command manager query suggestion
            add => CommandManager.RequerySuggested += value;

            // Remove event handler from command manager query suggestion
            remove => CommandManager.RequerySuggested -= value;
        }

        // Check if button can be active now based on condition
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        // Run stored action when button is clicked
        public void Execute(object? parameter) => _execute(parameter);
    }
}