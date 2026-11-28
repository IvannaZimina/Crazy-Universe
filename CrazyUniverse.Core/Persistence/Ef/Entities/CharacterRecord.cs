// This file defines the DB models for characters in EF Core.
// It uses TPH (Table-per-Hierarchy): all animal subclasses are stored in one Characters table
// and separated by the AnimalType discriminator column.

namespace CrazyUniverse.Core.Persistence.Ef.Entities;

// Base DB record shared by all animal types in the Characters table.
public abstract class CharacterRecord
{
    public string Id { get; set; } = string.Empty;      // Unique character ID (primary key, matches Animal.Id).

    public string Name { get; set; } = string.Empty;    // Character name.

    public string? Description { get; set; }            // Optional character description.

    public int Satiety { get; set; }                    // Saved satiety level (0..100).

    public string EnclosureId { get; set; } = string.Empty; // Foreign key pointing to the enclosure where this character lives.

    public EnclosureRecord? Enclosure { get; set; }         // Navigation reference to the parent enclosure record.
}

public sealed class MonkeyRecord : CharacterRecord { }  // DB record for Monkey (stored with AnimalType = "Monkey").

public sealed class LionRecord : CharacterRecord { }    // DB record for Lion (stored with AnimalType = "Lion").

public sealed class PenguinRecord : CharacterRecord { } // DB record for Penguin (stored with AnimalType = "Penguin").

public sealed class HoneyBadgerRecord : CharacterRecord { } // DB record for HoneyBadger (stored with AnimalType = "HoneyBadger").

public sealed class KangarooRecord : CharacterRecord { }    // DB record for Kangaroo (stored with AnimalType = "Kangaroo").

public sealed class DolphinRecord : CharacterRecord { }     // DB record for Dolphin (stored with AnimalType = "Dolphin").

public sealed class FrogRecord : CharacterRecord { }        // DB record for Frog (stored with AnimalType = "Frog").