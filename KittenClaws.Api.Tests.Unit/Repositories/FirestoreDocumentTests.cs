namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using System.Linq;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Utils;
using Google.Cloud.Firestore;
using Xunit;

public class FirestoreDocumentTests
{
    [Fact]
    public void ToDocument_OmitsUnsetFields()
    {
        var item = new TestRecord { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Text = "mock", CreatedTimestamp = "2026-01-01T00:00:00.000Z", UpdatedTimestamp = "2026-01-01T00:00:00.000Z" };

        Assert.Equal(new[] { "createdTimestamp", "id", "isDeleted", "text", "updatedTimestamp" }, FirestoreDocumentStore<TestRecord>.ToDocument(item).Keys.Order().ToArray());
        item.CreatedBy = "User2";
        Assert.Equal("User2", FirestoreDocumentStore<TestRecord>.ToDocument(item)["createdBy"]);
    }

    [Fact]
    public void ToUpdates_SetsTheTypesFieldsAndDeletesUnsetOnes()
    {
        var item = new TestRecord { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Text = "mock", CreatedTimestamp = "2026-01-01T00:00:00.000Z", UpdatedTimestamp = "2026-01-01T00:00:00.000Z" };

        var updates = FirestoreDocumentStore<TestRecord>.ToUpdates(item);

        Assert.Equal("mock", updates["text"]);
        Assert.Equal(FieldValue.Delete, updates["count"]);
        Assert.Equal(FieldValue.Delete, updates["updatedBy"]);
        Assert.False((bool)updates["isDeleted"]);
    }

    [Fact]
    public void ToDocument_StoresEachTypeAsItsFirestoreType()
    {
        var document = FirestoreDocumentStore<TestRecord>.ToDocument(TestRecord.Sample());

        Assert.Equal("stored", document["text"]);
        Assert.Equal(Fields.MaxSafeInteger, document["count"]);
        Assert.Equal(1.5, document["ratio"]);
        Assert.False(Assert.IsType<bool>(document["flag"]));
        Assert.Equal(new object?[] { "a", "b" }, Assert.IsType<List<object?>>(document["tags"]));
        Assert.Equal(new object?[] { 0L, -2L }, Assert.IsType<List<object?>>(document["counts"]));
        Assert.Equal(new object?[] { 0.25, 3L }, Assert.IsType<List<object?>>(document["ratios"]));
        Assert.Equal(new object?[] { true, false }, Assert.IsType<List<object?>>(document["flags"]));
        Assert.Empty(Assert.IsType<List<object?>>(document["empty"]));
    }

    [Fact]
    public void FromDocument_ReadsBackEveryTypeWithoutLoss()
    {
        var record = TestRecord.Sample();

        var read = FirestoreDocumentStore<TestRecord>.FromDocument(FirestoreDocumentStore<TestRecord>.ToDocument(record));

        Assert.Equal(Json.Serialize(record), Json.Serialize(read));
    }

    [Fact]
    public void FromDocument_ReadsTheValuesFirestoreReturns()
    {
        var read = FirestoreDocumentStore<TestRecord>.FromDocument(new Dictionary<string, object>
        {
            { "id", "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" },
            { "ratio", 2L },
            { "counts", new List<object> { 1L } },
            { "unknown", "kept out" },
        });

        Assert.Equal(2.0, read.Ratio);
        Assert.Equal(new long[] { 1 }, read.Counts);
        Assert.Null(read.Text);
    }
}
