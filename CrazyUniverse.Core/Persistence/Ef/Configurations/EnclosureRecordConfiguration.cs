// This file configures how EnclosureRecord is mapped to the Enclosures table in EF Core.
// Keeping EF rules here keeps domain models and ViewModels clean from database code.

using CrazyUniverse.Core.Persistence.Ef.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrazyUniverse.Core.Persistence.Ef.Configurations;

// EF Core table and relationship rules for EnclosureRecord.
public sealed class EnclosureRecordConfiguration : IEntityTypeConfiguration<EnclosureRecord>
{
    // Sets table name, primary key, column limits, and one-to-many relationship with characters.
    public void Configure(EntityTypeBuilder<EnclosureRecord> builder)
    {
        // Map this entity to the "Enclosures" table.
        builder.ToTable("Enclosures");

        // Configure primary key and max length for Id.
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasMaxLength(64)
            .IsRequired();

        // Name is required and limited to 100 characters.
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        // AnimalType key is required and limited to 64 characters.
        builder.Property(e => e.AnimalType)
            .HasMaxLength(64)
            .IsRequired();

        // One enclosure has many characters; deleting an enclosure cascades to its characters.
        builder.HasMany(e => e.Characters)
            .WithOne(c => c.Enclosure)
            .HasForeignKey(c => c.EnclosureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}