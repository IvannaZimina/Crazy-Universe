using System.Windows;
using CrazyUniverse.WpfApp.ViewModels;

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
    }
}