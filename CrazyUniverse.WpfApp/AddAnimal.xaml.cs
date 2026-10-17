using CrazyUniverse.Core.Resources;
using CrazyUniverse.WpfApp.ViewModels;
using System.Windows;

namespace CrazyUniverse.WpfApp
{
    // logic for AddAnimal.xaml
    public partial class AddAnimal : Window
    {
        // Readonly field to store ViewModel instance
        private readonly AddAnimalViewModel _viewModel;

        // Public property to expose selected animal type from ViewModel
        public string AnimalType => _viewModel.SelectedAnimalType;

        // Public property to expose trimmed animal name from ViewModel
        public string AnimalName => _viewModel.AnimalName.Trim();

        // Public property to expose trimmed animal description from ViewModel
        public string AnimalDescription => _viewModel.AnimalDescription.Trim();

        // Constructor of AddAnimal window class
        public AddAnimal()
        {
            // Initialize all WPF UI components defined in XAML file
            InitializeComponent();

            // Create new instance of AddAnimalViewModel
            _viewModel = new AddAnimalViewModel();

            // Set window DataContext to created ViewModel instance
            DataContext = _viewModel;

            // Subscribe to close request event from ViewModel to handle dialog closure
            _viewModel.CloseRequested += (result) =>
            {
                // Check if result is true and animal name is empty or whitespace
                if (result && string.IsNullOrWhiteSpace(AnimalName))
                {
                    // Show warning message box when animal name is required
                    MessageBox.Show(Messages.Err_AnimalNameRequired, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Set window DialogResult property
                DialogResult = result;

                // Close dialog window
                Close();
            };
        }
    }
}