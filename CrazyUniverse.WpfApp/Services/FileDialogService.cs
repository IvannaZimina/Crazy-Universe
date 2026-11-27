using System;
using System.Windows;
using Microsoft.Win32;

namespace CrazyUniverse.WpfApp.Services
{
    public sealed class FileDialogService : IFileDialogService
    {
        public string? ShowSaveFileDialog(string provider, string? defaultFileName = null)
        {
            var (filter, defaultExt) = GetFilterAndExtension(provider);

            var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                AddExtension = true,
                OverwritePrompt = true,
                FileName = string.IsNullOrWhiteSpace(defaultFileName)
                    ? $"crazy-world{defaultExt}"
                    : defaultFileName
            };

            Window? owner = Application.Current?.MainWindow;
            bool? result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }

        public string? ShowOpenFileDialog(string provider)
        {
            var (filter, defaultExt) = GetFilterAndExtension(provider);

            var dialog = new OpenFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                CheckFileExists = true,
                Multiselect = false
            };

            Window? owner = Application.Current?.MainWindow;
            bool? result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }

        private static (string Filter, string DefaultExt) GetFilterAndExtension(string provider)
        {
            if (string.Equals(provider?.Trim(), "Xml", StringComparison.OrdinalIgnoreCase))
            {
                return ("XML files (*.xml)|*.xml|All files (*.*)|*.*", ".xml");
            }

            return ("JSON files (*.json)|*.json|All files (*.*)|*.*", ".json");
        }
    }
}