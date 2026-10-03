namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using Xunit;

public class CatModelTests
{
    [Fact]
    public void Dto_HoldsTheIdFirstAndEveryShownField()
    {
        var properties = JsonAssert.Properties(CatSamples.Dto());

        Assert.Equal(new[] { "id", "tenantId", "region", "priority", "rank", "labels", "name", "breed", "ageYears", "weightKg", "indoor", "birthDate", "microchipId", "ownerEmail", "website", "tagCode", "tags", "scores", "adoptedAt", "lastVisit" }, properties.Keys);
        Assert.Equal(CatSamples.ItemId, properties["id"].GetString());
        JsonAssert.Equal("\"public\"", properties["tenantId"]);
        JsonAssert.Equal("\"eu\"", properties["region"]);
        JsonAssert.Equal("1", properties["priority"]);
        JsonAssert.Equal("1.5", properties["rank"]);
        JsonAssert.Equal("[]", properties["labels"]);
        JsonAssert.Equal("\"sample\"", properties["name"]);
        JsonAssert.Equal("\"tabby\"", properties["breed"]);
        JsonAssert.Equal("0", properties["ageYears"]);
        JsonAssert.Equal("1.5", properties["weightKg"]);
        JsonAssert.Equal("true", properties["indoor"]);
        JsonAssert.Equal("\"2026-01-01\"", properties["birthDate"]);
        JsonAssert.Equal("\"6f1c2a3b-4d5e-4f60-8a7b-000000000000\"", properties["microchipId"]);
        JsonAssert.Equal("\"unknown@example.com\"", properties["ownerEmail"]);
        JsonAssert.Equal("\"https://example.com/items/1\"", properties["website"]);
        JsonAssert.Equal("\"ABC-123\"", properties["tagCode"]);
        JsonAssert.Equal("[]", properties["tags"]);
        JsonAssert.Equal("[1]", properties["scores"]);
        JsonAssert.Equal("\"2026-01-15T10:00:00.000Z\"", properties["adoptedAt"]);
        JsonAssert.Equal("\"2026-01-01T00:00:00.000Z\"", properties["lastVisit"]);
    }

    [Fact]
    public void Dto_ReadsAMissingFieldAsItsStaticDefault()
    {
        var properties = JsonAssert.Properties(new CatDto(new CatEntity { Id = CatSamples.ItemId }));

        Assert.Equal(new[] { "id", "tenantId", "region", "priority", "rank", "labels", "name", "breed", "ageYears", "weightKg", "indoor", "birthDate", "microchipId", "ownerEmail", "website", "tagCode", "tags", "scores", "adoptedAt", "lastVisit" }, properties.Keys);
        JsonAssert.Equal("\"public\"", properties["tenantId"]);
        JsonAssert.Equal("\"eu\"", properties["region"]);
        JsonAssert.Equal("null", properties["priority"]);
        JsonAssert.Equal("null", properties["rank"]);
        JsonAssert.Equal("[]", properties["labels"]);
        JsonAssert.Equal("null", properties["name"]);
        JsonAssert.Equal("\"tabby\"", properties["breed"]);
        JsonAssert.Equal("0", properties["ageYears"]);
        JsonAssert.Equal("null", properties["weightKg"]);
        JsonAssert.Equal("true", properties["indoor"]);
        JsonAssert.Equal("null", properties["birthDate"]);
        JsonAssert.Equal("null", properties["microchipId"]);
        JsonAssert.Equal("\"unknown@example.com\"", properties["ownerEmail"]);
        JsonAssert.Equal("null", properties["website"]);
        JsonAssert.Equal("null", properties["tagCode"]);
        JsonAssert.Equal("[]", properties["tags"]);
        JsonAssert.Equal("null", properties["scores"]);
        JsonAssert.Equal("null", properties["adoptedAt"]);
        JsonAssert.Equal("\"2026-01-01T00:00:00.000Z\"", properties["lastVisit"]);
    }

    [Fact]
    public void CreateRequest_DefaultsFieldsTheBodyLeavesOut()
    {
        var item = new CreateCatRequest().ToEntity();
        JsonAssert.Equal("\"public\"", item.TenantId);
        JsonAssert.Equal("\"eu\"", item.Region);
        Assert.Null(item.Priority);
        Assert.Null(item.Rank);
        JsonAssert.Equal("[]", item.Labels);
        Assert.Null(item.Name);
        JsonAssert.Equal("\"tabby\"", item.Breed);
        JsonAssert.Equal("0", item.AgeYears);
        Assert.Null(item.WeightKg);
        JsonAssert.Equal("true", item.Indoor);
        Assert.NotNull(item.BirthDate);
        Assert.NotNull(item.MicrochipId);
        JsonAssert.Equal("\"unknown@example.com\"", item.OwnerEmail);
        Assert.Null(item.Website);
        Assert.Null(item.TagCode);
        JsonAssert.Equal("[]", item.Tags);
        Assert.Null(item.Scores);
        Assert.NotNull(item.AdoptedAt);
        JsonAssert.Equal("\"2026-01-01T00:00:00.000Z\"", item.LastVisit);
        JsonAssert.Equal("\"$none\"", item.Notes);
    }

    [Fact]
    public void ReplaceRequest_ResetsAcceptedFieldsAndKeepsTheRest()
    {
        var item = CatSamples.Entity();

        new ReplaceCatRequest().ApplyTo(item);

        Assert.Equal(CatSamples.StoredTimestamp, item.CreatedTimestamp);
        Assert.Equal("User2", item.CreatedBy);
        JsonAssert.Equal("\"public\"", item.TenantId);
        JsonAssert.Equal("\"eu\"", item.Region);
        Assert.Null(item.Priority);
        Assert.Null(item.Rank);
        JsonAssert.Equal("[]", item.Labels);
        Assert.Null(item.Name);
        JsonAssert.Equal("\"tabby\"", item.Breed);
        JsonAssert.Equal("0", item.AgeYears);
        Assert.Null(item.WeightKg);
        JsonAssert.Equal("true", item.Indoor);
        Assert.NotNull(item.BirthDate);
        JsonAssert.Equal("\"6f1c2a3b-4d5e-4f60-8a7b-000000000000\"", item.MicrochipId);
        JsonAssert.Equal("\"unknown@example.com\"", item.OwnerEmail);
        Assert.Null(item.Website);
        Assert.Null(item.TagCode);
        JsonAssert.Equal("[]", item.Tags);
        Assert.Null(item.Scores);
        Assert.NotNull(item.AdoptedAt);
        JsonAssert.Equal("\"2026-01-01T00:00:00.000Z\"", item.LastVisit);
        JsonAssert.Equal("\"$none\"", item.Notes);
    }

    [Fact]
    public void UpdateRequest_ChangesOnlyTheFieldsSent()
    {
        var item = CatSamples.Entity();

        new UpdateCatRequest().ApplyTo(item);

        Assert.Equal(Json.Serialize(CatSamples.Entity()), Json.Serialize(item));
    }

    [Theory]
    [InlineData("tenantId")]
    [InlineData("priority")]
    [InlineData("rank")]
    [InlineData("labels")]
    [InlineData("name")]
    [InlineData("ageYears")]
    [InlineData("weightKg")]
    [InlineData("indoor")]
    [InlineData("ownerEmail")]
    [InlineData("website")]
    [InlineData("tags")]
    [InlineData("adoptedAt")]
    [InlineData("notes")]
    public void UpdateRequest_SetsASentFieldEvenToNull(string name)
    {
        var request = new UpdateCatRequest();
        request.Sent.Add(name);
        var item = CatSamples.Entity();

        request.ApplyTo(item);

        Assert.False(JsonAssert.Properties(item).ContainsKey(name));
    }

    [Fact]
    public void CreateRequest_StoresDateTimesInUtc()
    {
        var request = new CreateCatRequest
        {
            AdoptedAt = "2026-01-31T11:30:00.1239+02:00",
            LastVisit = "2026-01-31T11:30:00.1239+02:00",
        };

        var item = request.ToEntity();

        Assert.Equal("2026-01-31T09:30:00.123Z", item.AdoptedAt);
        Assert.Equal("2026-01-31T09:30:00.123Z", item.LastVisit);
    }

    [Fact]
    public void ReplaceRequest_StoresDateTimesInUtc()
    {
        var request = new ReplaceCatRequest
        {
            AdoptedAt = "2026-01-31T11:30:00.1239+02:00",
            LastVisit = "2026-01-31T11:30:00.1239+02:00",
        };
        var item = new CatEntity();

        request.ApplyTo(item);

        Assert.Equal("2026-01-31T09:30:00.123Z", item.AdoptedAt);
        Assert.Equal("2026-01-31T09:30:00.123Z", item.LastVisit);
    }

    [Fact]
    public void UpdateRequest_StoresDateTimesInUtc()
    {
        var request = new UpdateCatRequest
        {
            AdoptedAt = "2026-01-31T11:30:00.1239+02:00",
        };
        request.Sent.UnionWith(["adoptedAt"]);
        var item = new CatEntity();

        request.ApplyTo(item);

        Assert.Equal("2026-01-31T09:30:00.123Z", item.AdoptedAt);
    }
}
