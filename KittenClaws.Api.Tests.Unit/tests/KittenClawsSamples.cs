namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public static class KittenClawsSamples
{
    public const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";

    public const string StoredTimestamp = "2026-01-01T00:00:00.000Z";

    public const string CreateBody = "{\"name\": \"sample\"}";

    public const string UpdateBody = "{\"name\": \"sample\"}";

    public static KittenClawsEntity Entity() => new()
    {
        Id = ItemId,
        CreatedTimestamp = StoredTimestamp,
        UpdatedTimestamp = StoredTimestamp,
        CreatedBy = "User2",
        UpdatedBy = "User3",
        Name = "sample",
    };

    public static KittenClawsDto Dto() => new(Entity());
}
