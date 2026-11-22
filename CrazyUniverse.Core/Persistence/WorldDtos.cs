using System.Collections.Generic;
using System.Xml.Serialization;

namespace CrazyUniverse.Core.Persistence
{
    // Plain data transfer objects (DTOs) used to save and load world state across JSON, XML, and database storages without domain logic, UI, or event subscriptions
    [XmlRoot("World")]
    public sealed class WorldDto
    {
        // Schema version of the saved world format
        [XmlAttribute("Version")]
        public int Version { get; set; } = 1;

        // List of enclosures (groups) in the world
        [XmlArray("Enclosures")]
        [XmlArrayItem("Enclosure")]
        public List<EnclosureDto> Enclosures { get; set; } = new();

        // Flat list of all saved characters so serialization and XML work cleanly without inheritance issues
        [XmlArray("Characters")]
        [XmlArrayItem("Character")]
        public List<CharacterDto> Characters { get; set; } = new();
    }

    // Represents a saved enclosure (group) and which animal type belongs to it
    public sealed class EnclosureDto
    {
        // Unique identifier of the enclosure (matches catalog enclosure id)
        [XmlElement("Id")]
        public string Id { get; set; } = string.Empty;

        // Display name of the enclosure
        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        // Stable animal class name (type key) associated with this enclosure
        [XmlElement("AnimalType")]
        public string AnimalType { get; set; } = string.Empty;
    }

    // Represents a single saved character and its current state
    public sealed class CharacterDto
    {
        // Unique character ID (GUID string)
        [XmlElement("Id")]
        public string Id { get; set; } = string.Empty;

        // Stable animal class name (e.g., Monkey, Lion) used to recreate the right subclass
        [XmlElement("Type")]
        public string Type { get; set; } = string.Empty;

        // Character name
        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        // Optional character description
        [XmlElement("Description", IsNullable = true)]
        public string? Description { get; set; }

        // Current satiety level (expected range is 0 to 100)
        [XmlElement("Satiety")]
        public int Satiety { get; set; }

        // ID of the enclosure this character belongs to
        [XmlElement("EnclosureId")]
        public string EnclosureId { get; set; } = string.Empty;
    }
}