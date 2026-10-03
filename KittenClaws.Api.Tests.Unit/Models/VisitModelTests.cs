namespace KittenClaws.Api.Tests.Unit;

using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using Xunit;

public class VisitModelTests
{
    [Fact]
    public void Dto_HoldsTheIdFirstAndEveryShownField()
    {
        var properties = JsonAssert.Properties(VisitSamples.Dto());

        Assert.Equal(new[] { "id", "tenantId", "region", "priority", "rank", "labels", "reason", "visitedOn", "cost", "paid", "checkedAt" }, properties.Keys);
        Assert.Equal(VisitSamples.ItemId, properties["id"].GetString());
        JsonAssert.Equal("\"public\"", properties["tenantId"]);
        JsonAssert.Equal("\"eu\"", properties["region"]);
        JsonAssert.Equal("1", properties["priority"]);
        JsonAssert.Equal("1.5", properties["rank"]);
        JsonAssert.Equal("[]", properties["labels"]);
        JsonAssert.Equal("\"sample\"", properties["reason"]);
        JsonAssert.Equal("\"2026-01-01\"", properties["visitedOn"]);
        JsonAssert.Equal("1.5", properties["cost"]);
        JsonAssert.Equal("false", properties["paid"]);
        JsonAssert.Equal("[\"2026-01-15T10:00:00.000Z\"]", properties["checkedAt"]);
    }

    [Fact]
    public void Dto_ReadsAMissingFieldAsItsStaticDefault()
    {
        var properties = JsonAssert.Properties(new VisitDto(new VisitEntity { Id = VisitSamples.ItemId }));

        Assert.Equal(new[] { "id", "tenantId", "region", "priority", "rank", "labels", "reason", "visitedOn", "cost", "paid", "checkedAt" }, properties.Keys);
        JsonAssert.Equal("\"public\"", properties["tenantId"]);
        JsonAssert.Equal("\"eu\"", properties["region"]);
        JsonAssert.Equal("null", properties["priority"]);
        JsonAssert.Equal("null", properties["rank"]);
        JsonAssert.Equal("[]", properties["labels"]);
        JsonAssert.Equal("null", properties["reason"]);
        JsonAssert.Equal("null", properties["visitedOn"]);
        JsonAssert.Equal("null", properties["cost"]);
        JsonAssert.Equal("false", properties["paid"]);
        JsonAssert.Equal("null", properties["checkedAt"]);
    }

    [Fact]
    public void CreateRequest_DefaultsFieldsTheBodyLeavesOut()
    {
        var item = new CreateVisitRequest().ToEntity();
        JsonAssert.Equal("\"public\"", item.TenantId);
        JsonAssert.Equal("\"eu\"", item.Region);
        Assert.Null(item.Priority);
        Assert.Null(item.Rank);
        JsonAssert.Equal("[]", item.Labels);
        Assert.Null(item.Reason);
        Assert.Null(item.VisitedOn);
        Assert.Null(item.Cost);
        JsonAssert.Equal("false", item.Paid);
        Assert.Null(item.CheckedAt);
    }

    [Fact]
    public void UpdateRequest_ChangesOnlyTheFieldsSent()
    {
        var item = VisitSamples.Entity();

        new UpdateVisitRequest().ApplyTo(item);

        Assert.Equal(Json.Serialize(VisitSamples.Entity()), Json.Serialize(item));
    }

    [Theory]
    [InlineData("tenantId")]
    [InlineData("priority")]
    [InlineData("rank")]
    [InlineData("labels")]
    [InlineData("visitedOn")]
    [InlineData("cost")]
    [InlineData("paid")]
    [InlineData("checkedAt")]
    public void UpdateRequest_SetsASentFieldEvenToNull(string name)
    {
        var request = new UpdateVisitRequest();
        request.Sent.Add(name);
        var item = VisitSamples.Entity();

        request.ApplyTo(item);

        Assert.False(JsonAssert.Properties(item).ContainsKey(name));
    }

    [Fact]
    public void UpdateRequest_StoresDateTimesInUtc()
    {
        var request = new UpdateVisitRequest
        {
            CheckedAt = ["2026-01-31T11:30:00.1239+02:00"],
        };
        request.Sent.UnionWith(["checkedAt"]);
        var item = new VisitEntity();

        request.ApplyTo(item);

        Assert.Equal(new[] { "2026-01-31T09:30:00.123Z" }, item.CheckedAt);
    }
}
