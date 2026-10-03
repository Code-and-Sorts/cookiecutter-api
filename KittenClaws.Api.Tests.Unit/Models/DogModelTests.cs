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

        Assert.Equal(new[] { "id", "name" }, properties.Keys);
        Assert.Equal(DogSamples.ItemId, properties["id"].GetString());
        JsonAssert.Equal("\"sample\"", properties["name"]);
    }

    [Fact]
    public void Dto_ReadsAMissingFieldAsItsStaticDefault()
    {
        var properties = JsonAssert.Properties(new DogDto(new DogEntity { Id = DogSamples.ItemId }));

        Assert.Equal(new[] { "id", "name" }, properties.Keys);
        JsonAssert.Equal("null", properties["name"]);
    }

    [Fact]
    public void CreateRequest_DefaultsFieldsTheBodyLeavesOut()
    {
        var item = new CreateDogRequest().ToEntity();
        Assert.Null(item.Name);
    }

    [Fact]
    public void ReplaceRequest_ResetsAcceptedFieldsAndKeepsTheRest()
    {
        var item = DogSamples.Entity();

        new ReplaceDogRequest().ApplyTo(item);

        Assert.Equal(DogSamples.StoredTimestamp, item.CreatedTimestamp);
        Assert.Equal("User2", item.CreatedBy);
        Assert.Null(item.Name);
    }
}
