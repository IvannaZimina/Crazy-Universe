using System;
using System.Windows;
using CrazyUniverse.WpfApp.ViewModels;

namespace CrazyUniverse.WpfApp
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        // Receives MainViewModel from DI and sets it as the window DataContext[cite: 24, 28]
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}