namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using Xunit;

public class KittenClawsModelTests
{
    [Fact]
    public void Dto_HoldsTheIdFirstAndEveryShownField()
    {
        var properties = JsonAssert.Properties(KittenClawsSamples.Dto());

        Assert.Equal(new[] { "id", "name" }, properties.Keys);
        Assert.Equal(KittenClawsSamples.ItemId, properties["id"].GetString());
        JsonAssert.Equal("\"sample\"", properties["name"]);
    }

    [Fact]
    public void Dto_ReadsAMissingFieldAsItsStaticDefault()
    {
        var properties = JsonAssert.Properties(new KittenClawsDto(new KittenClawsEntity { Id = KittenClawsSamples.ItemId }));

        Assert.Equal(new[] { "id", "name" }, properties.Keys);
        JsonAssert.Equal("null", properties["name"]);
    }

    [Fact]
    public void CreateRequest_DefaultsFieldsTheBodyLeavesOut()
    {
        var item = new CreateKittenClawsRequest().ToEntity();
        Assert.Null(item.Name);
    }

    [Fact]
    public void UpdateRequest_ChangesOnlyTheFieldsSent()
    {
        var item = KittenClawsSamples.Entity();

        new UpdateKittenClawsRequest().ApplyTo(item);

        Assert.Equal(Json.Serialize(KittenClawsSamples.Entity()), Json.Serialize(item));
    }

    [Theory]
    [InlineData("name")]
    public void UpdateRequest_SetsASentFieldEvenToNull(string name)
    {
        var request = new UpdateKittenClawsRequest();
        request.Sent.Add(name);
        var item = KittenClawsSamples.Entity();

        request.ApplyTo(item);

        Assert.False(JsonAssert.Properties(item).ContainsKey(name));
    }
}
