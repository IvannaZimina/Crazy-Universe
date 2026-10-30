using System;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.Core.Events
{
    // Перечисление типов событий для логов
    public enum EventKind
    {
        Add,
        Remove,
        CrazyAction,
        Reaction
    }

    // Типизированные аргументы для события логирования
    public class LogEventArgs : EventArgs
    {
        public EventKind Kind { get; }
        public Animal Cause { get; }
        public string Message { get; }

        public LogEventArgs(EventKind kind, Animal cause, string message)
        {
            Kind = kind;
            Cause = cause;
            Message = message;
        }
    }
}