using System.Globalization; // Needed for work with languages and regions
using System.Threading;     // Needed to manage current program threads

namespace CrazyUniverse.Core.Helpers
{
    // Helper class to change app language
    public static class LocalizationHelper
    {
        // Method to set a new culture (takes code like "ru-RU" or "en-US")
        public static void SetCulture(string cultureCode)
        {
            // Create culture object from the given text code
            CultureInfo culture = new CultureInfo(cultureCode);

            // Change UI language for the active thread (so text files change)
            Thread.CurrentThread.CurrentUICulture = culture;

            // Change general culture for the active thread (for numbers, dates, etc.)
            Thread.CurrentThread.CurrentCulture = culture;

            // Set default UI language for any new threads in the future
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Set default general culture for any new threads in the future
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }
    }
}