namespace CrazyUniverse.WpfApp.Services
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string provider, string? defaultFileName = null);
        string? ShowOpenFileDialog(string provider);
    }
}