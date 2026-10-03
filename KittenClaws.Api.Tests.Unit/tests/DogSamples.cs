namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public static class DogSamples
{
    public const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";

    public const string StoredTimestamp = "2026-01-01T00:00:00.000Z";

    public const string CreateBody = "{\"labels\": [], \"name\": \"sample\", \"priority\": 1, \"rank\": 1.5, \"region\": \"eu\", \"tenantId\": \"public\"}";

    public const string ReplaceBody = "{\"labels\": [], \"name\": \"sample\", \"priority\": 1, \"rank\": 1.5, \"tenantId\": \"public\"}";

    public static DogEntity Entity() => new()
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
    };

    public static DogDto Dto() => new(Entity());
}
