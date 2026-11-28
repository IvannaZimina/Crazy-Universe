// This file defines the DB model for an animal enclosure (group).
// It maps to the Enclosures table in EF Core and holds the animals inside that enclosure.

using System.Collections.Generic;

namespace CrazyUniverse.Core.Persistence.Ef.Entities;

// Database record for one enclosure (plain data only, no game logic).
public sealed class EnclosureRecord
{
    public string Id { get; set; } = string.Empty;          // Unique enclosure ID (primary key in the Enclosures table).

    public string Name { get; set; } = string.Empty;        // Display name of the enclosure (for example, "Monkeys").

    public string AnimalType { get; set; } = string.Empty;  // Stable animal type key allowed in this enclosure (for ex., "Monkey").

    public List<CharacterRecord> Characters { get; set; } = []; // Navigation list of all characters that belong to this enclosure
}