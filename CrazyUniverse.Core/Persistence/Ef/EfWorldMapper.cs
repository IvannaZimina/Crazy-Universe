// This file converts between WorldDto (used by IWorldStorage) and EF Core TPH entities.
// Having a separate mapper keeps EfWorldStorage focused on database read/write operations.

using CrazyUniverse.Core.Persistence;
using CrazyUniverse.Core.Persistence.Ef.Entities;
using CrazyUniverse.Core.Resources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyUniverse.Core.Persistence.Ef;

// Maps WorldDto to/from EF Core EnclosureRecord and CharacterRecord (TPH) entities.
internal static class EfWorldMapper
{
    // Converts a WorldDto into EF Core enclosure and character records.
    public static (List<EnclosureRecord> Enclosures, List<CharacterRecord> Characters) ToRecords(WorldDto world)
    {
        // Ensure the DTO object exists before reading its lists.
        ArgumentNullException.ThrowIfNull(world);

        // Convert enclosures first so characters can link to them via EnclosureId.
        var enclosures = (world.Enclosures ?? [])
            .Select(e => new EnclosureRecord
            {
                Id = e.Id,
                Name = e.Name,
                AnimalType = e.AnimalType
            })
            .ToList();

        // Convert each CharacterDto into its matching TPH subclass record.
        var characters = (world.Characters ?? [])
            .Select(c =>
            {
                CharacterRecord record = CreateRecordByType(c.Type);
                record.Id = c.Id;
                record.Name = c.Name;
                record.Description = c.Description;
                record.Satiety = c.Satiety;
                record.EnclosureId = c.EnclosureId;
                return record;
            })
            .ToList();

        return (enclosures, characters);
    }

    // Converts database records back into a WorldDto for validation and world restoration.
    public static WorldDto ToWorldDto(
        IEnumerable<EnclosureRecord> enclosures,
        IEnumerable<CharacterRecord> characters,
        int version = WorldValidator.SupportedVersion)
    {
        var enclosureDtos = enclosures
            .Select(e => new EnclosureDto
            {
                Id = e.Id,
                Name = e.Name,
                AnimalType = e.AnimalType
            })
            .ToList();

        var characterDtos = characters
            .Select(c => new CharacterDto
            {
                Id = c.Id,
                Type = GetTypeKey(c),
                Name = c.Name,
                Description = c.Description,
                Satiety = c.Satiety,
                EnclosureId = c.EnclosureId
            })
            .ToList();

        return new WorldDto
        {
            Version = version,
            Enclosures = enclosureDtos,
            Characters = characterDtos
        };
    }

    // Creates the concrete TPH record class for a given animal type key.
    private static CharacterRecord CreateRecordByType(string? typeKey)
    {
        string normalizedType = typeKey?.Trim() ?? string.Empty;

        return normalizedType switch
        {
            "Monkey" => new MonkeyRecord(),
            "Lion" => new LionRecord(),
            "Penguin" => new PenguinRecord(),
            "HoneyBadger" => new HoneyBadgerRecord(),
            "Kangaroo" => new KangarooRecord(),
            "Dolphin" => new DolphinRecord(),
            "Frog" => new FrogRecord(),
            _ => throw new InvalidWorldDataException(
                string.Format(Messages.Err_UnknownAnimalType, normalizedType))
        };
    }

    // Maps a TPH record instance back to its stable animal type key.
    private static string GetTypeKey(CharacterRecord record) => record switch
    {
        MonkeyRecord => "Monkey",
        LionRecord => "Lion",
        PenguinRecord => "Penguin",
        HoneyBadgerRecord => "HoneyBadger",
        KangarooRecord => "Kangaroo",
        DolphinRecord => "Dolphin",
        FrogRecord => "Frog",
        _ => throw new InvalidWorldDataException(
            string.Format(Messages.Err_UnknownRecordType, record.GetType().Name))
    };
}