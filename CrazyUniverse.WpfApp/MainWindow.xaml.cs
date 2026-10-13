using System.Windows;
using CrazyUniverse.WpfApp.ViewModels;

namespace CrazyUniverse.WpfApp
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        // Constructor of the main window class
        public MainWindow()
        {
            // Initialize all WPF UI components defined in the XAML file
            InitializeComponent();

            // Set DataContext of the window to an instance of MainViewModel
            DataContext = new MainViewModel();
        }
    }
}