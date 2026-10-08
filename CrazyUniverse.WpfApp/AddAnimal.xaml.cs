using System.Collections.Generic;
using System.Windows;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.WpfApp
{
    public partial class AddAnimal : Window
    {
        // Возвращаем выбранный технический ключ (например, "Monkey", "Lion"), который привязан как Key в словаре
        public string AnimalType => CmbAnimalType.SelectedValue as string ?? "Monkey";
        public string AnimalName => TxtBoxName.Text.Trim();
        public string AnimalDescription => TxtBoxDescription.Text.Trim();

        public AddAnimal()
        {
            InitializeComponent();

            // Dictionary to map technical class names to localized display names for the ComboBox
            // Key : Technical class name (used in code), Value: Localized display name (shown to user)
            var animalOptions = new Dictionary<string, string>
            {
                { "Monkey", Messages.UI_MonkeyTypeName },
                { "Lion", Messages.UI_LionTypeName },
                { "Penguin", Messages.UI_PenguinTypeName },
                { "HoneyBadger", Messages.UI_HoneyBadgerTypeName }
            };

            CmbAnimalType.ItemsSource = animalOptions;
            CmbAnimalType.DisplayMemberPath = "Value";   // what user sees in the dropdown (localized name)
            CmbAnimalType.SelectedValuePath = "Key";     // System key for the selected item, which will be used in AnimalType property

            if (CmbAnimalType.Items.Count > 0)
            {
                CmbAnimalType.SelectedIndex = 0;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AnimalName))
            {
                MessageBox.Show(Messages.Err_AnimalNameRequired, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}