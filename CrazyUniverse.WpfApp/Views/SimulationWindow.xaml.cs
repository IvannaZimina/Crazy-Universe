using CrazyUniverse.Core.Models;
using CrazyUniverse.WpfApp.ViewModels;
using System.Windows;

namespace CrazyUniverse.WpfApp.Views
{
    public partial class SimulationWindow : Window
    {
        public SimulationWindow()
        {
            InitializeComponent();
            DataContext = new SimulationViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        public SimulationWindow(IEnumerable<Animal> animals)
        {
            InitializeComponent();
            DataContext = new SimulationViewModel(animals);
        }
    }
}