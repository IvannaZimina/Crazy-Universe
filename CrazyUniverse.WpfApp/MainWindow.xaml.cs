using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CrazyUniverse.Core.Helpers;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.WpfApp
{
    public partial class MainWindow : Window
    {
        // ObservableCollection to store and track animals dynamically in the UI => from the task
        public ObservableCollection<Animal> Animals { get; set; }

        // Constructor of the main window class
        public MainWindow()
        {
            // Initialize all WPF UI components defined in the XAML file
            InitializeComponent();

            // Initialize the default collection of animals using polymorphic concrete classes
            Animals = new ObservableCollection<Animal>
            {
                new Monkey("George"),
                new Lion("Simba"),
                new Penguin("Skipper")
            };

            // Bind the animal collection to the ListBox control in the UI
            AnimalsListBox.ItemsSource = Animals;

            // Update population and total animal counter labels
            UpdateCounters();

            // Log the initial application startup message using resource strings
            Log(CrazyUniverse.Core.Resources.Messages.Log_ZooInit);
        }

        /* Event handler triggered when the user selects a different item in the ListBox */
        private void AnimalsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Checks if the user actually clicked on an animal in the list. 
            // If so, captures it into 'selectedAnimal'; if nothing is selected, safely does nothing.
            if (AnimalsListBox.SelectedItem is Animal selectedAnimal)
            {
                // Dynamically set Feed button content using FeedCost from Core model
                BtnFeed.Content = $"Feed (+{selectedAnimal.FeedCost})";

                // Swim interface check (e.g., Penguin)
                if (selectedAnimal is ISwimmable swimmable)
                {
                    BtnSwim.Visibility = Visibility.Visible;
                    LblSwim.Visibility = Visibility.Visible;
                    // Dynamically set button text using SwimCost from Core
                    BtnSwim.Content = $"Swim (-{swimmable.SwimCost})";
                }
                else
                {
                    BtnSwim.Visibility = Visibility.Collapsed;
                    LblSwim.Visibility = Visibility.Collapsed;
                }

                // Jump interface check (e.g., Monkey)
                if (selectedAnimal is IJumpable jumpable)
                {
                    BtnJump.Visibility = Visibility.Visible;
                    LblJump.Visibility = Visibility.Visible;
                    // Dynamically set button text using JumpCost from Core
                    BtnJump.Content = $"Jump (-{jumpable.JumpCost})";
                }
                else
                {
                    BtnJump.Visibility = Visibility.Collapsed;
                    LblJump.Visibility = Visibility.Collapsed;
                }

                // Vocalizable interface check (e.g., Lion, Monkey)
                if (selectedAnimal is IVocalizable vocalizable)
                {
                    BtnMakeSound.Visibility = Visibility.Visible;
                    LblSound.Visibility = Visibility.Visible;
                    // Dynamically set button text using SoundCost from Core
                    BtnMakeSound.Content = $"Make Sound (-{vocalizable.SoundCost})";
                }
                else
                {
                    BtnMakeSound.Visibility = Visibility.Collapsed;
                    LblSound.Visibility = Visibility.Collapsed;
                }
            }
        }

        /* Event handler for the button that adds a new animal to the collection */
        private void AddAnimal_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of Monkey with a dynamically generated name based on count
            var newAnimal = new Monkey($"Monkey #{Animals.Count + 1}");

            // Add the newly created animal to the observable collection
            Animals.Add(newAnimal);

            // Refresh the population counters displayed in the header and footer
            UpdateCounters();

            // Record the action into the activity log using resource string format
            Log(string.Format(CrazyUniverse.Core.Resources.Messages.Log_AddedAnimal, newAnimal.Name));
        }

        /* Event handler for the button that removes the currently selected animal */
        private void RemoveAnimal_Click(object sender, RoutedEventArgs e)
        {
            // Check if an animal is actually selected in the ListBox
            if (AnimalsListBox.SelectedItem is Animal selectedAnimal)
            {
                string name = selectedAnimal.Name;

                // Remove the selected animal from the collection
                Animals.Remove(selectedAnimal);

                // Update the counters in the UI
                UpdateCounters();

                // Log the removal action with the animal's name using resource format
                Log(string.Format(CrazyUniverse.Core.Resources.Messages.Log_RemovedAnimal, name));
            }
            else
            {
                // Log a warning message if no animal was selected using resource string
                Log(CrazyUniverse.Core.Resources.Messages.Log_NoAnimalRemove);
            }
        }

        /* Event handler for feeding the selected animal */
        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            // Check if an animal is currently selected in the list
            if (AnimalsListBox.SelectedItem is Animal selectedAnimal)
            {
                // Call the Feed method from the Core domain logic (increases satiety)
                selectedAnimal.Feed();

                // Refresh the UI bindings to update progress bars
                RefreshUI();

                // Log the feeding action and the updated satiety value using resource format
                Log(string.Format(CrazyUniverse.Core.Resources.Messages.Action_AnimalFed, selectedAnimal.Name, selectedAnimal.Satiety));
            }
            else
            {
                // Log a warning if the user clicked feed without selecting an animal using resource string
                Log(CrazyUniverse.Core.Resources.Messages.Log_NoAnimalFeed);
            }
        }

        /* Event handler for triggering the polymorphic crazy action of the selected animal */
        private void CrazyAction_Click(object sender, RoutedEventArgs e)
        {
            // Check if an animal is currently selected
            if (AnimalsListBox.SelectedItem is Animal selectedAnimal)
            {
                // Invoke the polymorphic CrazyAction method and capture its result message
                string resultMessage = selectedAnimal.CrazyAction();

                // Refresh UI controls to reflect state changes
                RefreshUI();

                // Record the action's result message into the activity log journal
                Log(resultMessage);
            }
            else
            {
                // Log a warning if no animal is selected using resource string
                Log(CrazyUniverse.Core.Resources.Messages.Log_NoAnimalCrazy);
            }
        }

        /* Event handler for swimming action (ISwimmable) */
        private void Swim_Click(object sender, RoutedEventArgs e)
        {
            if (AnimalsListBox.SelectedItem is ISwimmable swimmableAnimal)
            {
                string resultMessage = swimmableAnimal.Swim(15);
                RefreshUI();
                Log(resultMessage);
            }
        }

        /* Event handler for jumping action (IJumpable) */
        private void Jump_Click(object sender, RoutedEventArgs e)
        {
            if (AnimalsListBox.SelectedItem is IJumpable jumpableAnimal)
            {
                string resultMessage = jumpableAnimal.Jump();
                RefreshUI();
                Log(resultMessage);
            }
        }

        /* Event handler for making sound action (IVocalizable) */
        private void MakeSound_Click(object sender, RoutedEventArgs e)
        {
            if (AnimalsListBox.SelectedItem is IVocalizable vocalizableAnimal)
            {
                string resultMessage = vocalizableAnimal.MakeSound();
                RefreshUI();
                Log(resultMessage);
            }
        }

        /* Event handler for language switching buttons in XAML */
        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            // Checks if the sender is actually a Button and its Tag property contains a string (culture code)
            if (sender is Button button && button.Tag is string cultureCode)
            {
                // Set the selected culture globally using the helper from the Core project
                LocalizationHelper.SetCulture(cultureCode);

                // Record the language change event into the log using resource string format
                Log(string.Format(CrazyUniverse.Core.Resources.Messages.Log_LangChanged, cultureCode));
            }
        }

        /* Forces UI elements (like ProgressBar) to refresh when underlying properties change */
        private void RefreshUI()
        {
            // Store the currently selected animal temporarily
            var selected = AnimalsListBox.SelectedItem;

            // Clear the selection to break binding cache
            AnimalsListBox.SelectedItem = null;

            // Re-assign the selected animal to trigger UI updates and re-binding
            AnimalsListBox.SelectedItem = selected;
        }

        /* Updates the population and total animal counters in the UI */
        private void UpdateCounters()
        {
            // Get the current number of animals in the collection
            int count = Animals.Count;

            // Update the top header population text block using resource format
            PopulationText.Text = string.Format(CrazyUniverse.Core.Resources.Messages.UI_ZooPopulation, count);

            // Update the left column total animals text block using resource format
            TotalAnimalsText.Text = string.Format(CrazyUniverse.Core.Resources.Messages.UI_TotalAnimals, count);
        }

        /* Registers meaningful messages into the UI activity log journal */
        private void Log(string message)
        {
            // Append the timestamped message to the TextBox log control
            LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");

            // Automatically scroll the text box down to show the latest log entry
            LogTextBox.ScrollToEnd();
        }
    }
}