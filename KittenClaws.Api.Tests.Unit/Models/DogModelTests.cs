namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Requests;
using Xunit;

public class DogModelTests
{
    [Fact]
    public void Dto_HoldsTheIdFirstAndEveryShownField()
    {
        var properties = JsonAssert.Properties(DogSamples.Dto());

        Assert.Equal(new[] { "id", "tenantId", "region", "priority", "rank", "labels", "name" }, properties.Keys);
        Assert.Equal(DogSamples.ItemId, properties["id"].GetString());
        JsonAssert.Equal("\"public\"", properties["tenantId"]);
        JsonAssert.Equal("\"eu\"", properties["region"]);
        JsonAssert.Equal("1", properties["priority"]);
        JsonAssert.Equal("1.5", properties["rank"]);
        JsonAssert.Equal("[]", properties["labels"]);
        JsonAssert.Equal("\"sample\"", properties["name"]);
    }

    [Fact]
    public void Dto_ReadsAMissingFieldAsItsStaticDefault()
    {
        var properties = JsonAssert.Properties(new DogDto(new DogEntity { Id = DogSamples.ItemId }));

        Assert.Equal(new[] { "id", "tenantId", "region", "priority", "rank", "labels", "name" }, properties.Keys);
        JsonAssert.Equal("\"public\"", properties["tenantId"]);
        JsonAssert.Equal("\"eu\"", properties["region"]);
        JsonAssert.Equal("null", properties["priority"]);
        JsonAssert.Equal("null", properties["rank"]);
        JsonAssert.Equal("[]", properties["labels"]);
        JsonAssert.Equal("null", properties["name"]);
    }

    [Fact]
    public void CreateRequest_DefaultsFieldsTheBodyLeavesOut()
    {
        var item = new CreateDogRequest().ToEntity();
        JsonAssert.Equal("\"public\"", item.TenantId);
        JsonAssert.Equal("\"eu\"", item.Region);
        Assert.Null(item.Priority);
        Assert.Null(item.Rank);
        JsonAssert.Equal("[]", item.Labels);
        Assert.Null(item.Name);
    }

    [Fact]
    public void ReplaceRequest_ResetsAcceptedFieldsAndKeepsTheRest()
    {
        var item = DogSamples.Entity();

        new ReplaceDogRequest().ApplyTo(item);

        Assert.Equal(DogSamples.StoredTimestamp, item.CreatedTimestamp);
        Assert.Equal("User2", item.CreatedBy);
        JsonAssert.Equal("\"public\"", item.TenantId);
        JsonAssert.Equal("\"eu\"", item.Region);
        Assert.Null(item.Priority);
        Assert.Null(item.Rank);
        JsonAssert.Equal("[]", item.Labels);
        Assert.Null(item.Name);
    }
}
