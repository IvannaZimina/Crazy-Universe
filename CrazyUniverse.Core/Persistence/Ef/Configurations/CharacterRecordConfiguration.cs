// This file configures how CharacterRecord and its subclasses map to the Characters table.
// It sets keys, column limits, foreign key to Enclosures, and the TPH discriminator column.

using CrazyUniverse.Core.Persistence.Ef.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrazyUniverse.Core.Persistence.Ef.Configurations;

// EF Core table and TPH inheritance rules for CharacterRecord.
public sealed class CharacterRecordConfiguration : IEntityTypeConfiguration<CharacterRecord>
{
    // Sets columns, validation limits, and TPH discriminator values for all animal types.
    public void Configure(EntityTypeBuilder<CharacterRecord> builder)
    {
        // Store all animal record subclasses in a single "Characters" table.
        builder.ToTable("Characters");

        // Configure primary key and max length for Id.
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasMaxLength(64)
            .IsRequired();

        // Name is required and matches the 100-character domain validation rule.
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Description is optional and matches the 500-character domain validation rule.
        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // Satiety is a required integer column.
        builder.Property(c => c.Satiety)
            .IsRequired();

        // Foreign key to Enclosures is required.
        builder.Property(c => c.EnclosureId)
            .HasMaxLength(64)
            .IsRequired();

        // Configure TPH inheritance: EF uses the "AnimalType" column to tell subclasses apart.
        builder.HasDiscriminator<string>("AnimalType")
            .HasValue<MonkeyRecord>("Monkey")
            .HasValue<LionRecord>("Lion")
            .HasValue<PenguinRecord>("Penguin")
            .HasValue<HoneyBadgerRecord>("HoneyBadger")
            .HasValue<KangarooRecord>("Kangaroo")
            .HasValue<DolphinRecord>("Dolphin")
            .HasValue<FrogRecord>("Frog");
    }
}