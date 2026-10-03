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

        Assert.Equal(new[] { "id", "name" }, properties.Keys);
        Assert.Equal(CatSamples.ItemId, properties["id"].GetString());
        JsonAssert.Equal("\"sample\"", properties["name"]);
    }

    [Fact]
    public void Dto_ReadsAMissingFieldAsItsStaticDefault()
    {
        var properties = JsonAssert.Properties(new CatDto(new CatEntity { Id = CatSamples.ItemId }));

        Assert.Equal(new[] { "id", "name" }, properties.Keys);
        JsonAssert.Equal("null", properties["name"]);
    }

    [Fact]
    public void CreateRequest_DefaultsFieldsTheBodyLeavesOut()
    {
        var item = new CreateCatRequest().ToEntity();
        Assert.Null(item.Name);
    }

    [Fact]
    public void UpdateRequest_ChangesOnlyTheFieldsSent()
    {
        var item = CatSamples.Entity();

        new UpdateCatRequest().ApplyTo(item);

        Assert.Equal(Json.Serialize(CatSamples.Entity()), Json.Serialize(item));
    }

    [Theory]
    [InlineData("name")]
    public void UpdateRequest_SetsASentFieldEvenToNull(string name)
    {
        var request = new UpdateCatRequest();
        request.Sent.Add(name);
        var item = CatSamples.Entity();

        request.ApplyTo(item);

        Assert.False(JsonAssert.Properties(item).ContainsKey(name));
    }
}
