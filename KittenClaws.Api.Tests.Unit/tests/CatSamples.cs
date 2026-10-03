namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public static class CatSamples
{
    public const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";

    public const string StoredTimestamp = "2026-01-01T00:00:00.000Z";

    public const string CreateBody = "{\"adoptedAt\": \"2026-01-15T10:00:00.000Z\", \"ageYears\": 0, \"birthDate\": \"2026-01-01\", \"breed\": \"tabby\", \"indoor\": true, \"labels\": [], \"lastVisit\": \"2026-01-01T00:00:00.000Z\", \"microchipId\": \"6f1c2a3b-4d5e-4f60-8a7b-000000000000\", \"name\": \"sample\", \"notes\": \"$none\", \"ownerEmail\": \"unknown@example.com\", \"priority\": 1, \"rank\": 1.5, \"region\": \"eu\", \"scores\": [1], \"tagCode\": \"ABC-123\", \"tags\": [], \"tenantId\": \"public\", \"website\": \"https://example.com/items/1\", \"weightKg\": 1.5}";

    public const string ReplaceBody = "{\"adoptedAt\": \"2026-01-15T10:00:00.000Z\", \"ageYears\": 0, \"birthDate\": \"2026-01-01\", \"breed\": \"tabby\", \"indoor\": true, \"labels\": [], \"lastVisit\": \"2026-01-01T00:00:00.000Z\", \"name\": \"sample\", \"notes\": \"$none\", \"ownerEmail\": \"unknown@example.com\", \"priority\": 1, \"rank\": 1.5, \"scores\": [1], \"tagCode\": \"ABC-123\", \"tags\": [], \"tenantId\": \"public\", \"website\": \"https://example.com/items/1\", \"weightKg\": 1.5}";

    public const string UpdateBody = "{\"adoptedAt\": \"2026-01-15T10:00:00.000Z\", \"ageYears\": 0, \"indoor\": true, \"labels\": [], \"name\": \"sample\", \"notes\": \"$none\", \"ownerEmail\": \"unknown@example.com\", \"priority\": 1, \"rank\": 1.5, \"tags\": [], \"tenantId\": \"public\", \"website\": \"https://example.com/items/1\", \"weightKg\": 1.5}";

    public static CatEntity Entity() => new()
    {
        Id = ItemId,
        CreatedTimestamp = StoredTimestamp,
        UpdatedTimestamp = StoredTimestamp,
        CreatedBy = "User2",
        UpdatedBy = "User3",
        TenantId = "public",
        Region = "eu",
        Priority = 1,
        Rank = 1.5,
        Labels = [],
        Name = "sample",
        Breed = "tabby",
        AgeYears = 0,
        WeightKg = 1.5,
        Indoor = true,
        BirthDate = "2026-01-01",
        MicrochipId = "6f1c2a3b-4d5e-4f60-8a7b-000000000000",
        OwnerEmail = "unknown@example.com",
        Website = "https://example.com/items/1",
        TagCode = "ABC-123",
        Tags = [],
        Scores = [1],
        AdoptedAt = "2026-01-15T10:00:00.000Z",
        LastVisit = "2026-01-01T00:00:00.000Z",
        Notes = "$none",
    };

    public static CatDto Dto() => new(Entity());
}
