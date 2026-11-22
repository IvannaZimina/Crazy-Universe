using System;

namespace CrazyUniverse.Core.Persistence
{
    // Custom exception thrown when loaded or mapped world data is corrupted, missing required fields, or violates domain/validation rules
    public sealed class InvalidWorldDataException : Exception
    {
        // Creates a new exception with a descriptive error message
        public InvalidWorldDataException(string message) : base(message)
        {
        }

        // Creates a new exception wrapping an underlying error (such as a JSON/XML parse or format exception) while preserving its details
        public InvalidWorldDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}