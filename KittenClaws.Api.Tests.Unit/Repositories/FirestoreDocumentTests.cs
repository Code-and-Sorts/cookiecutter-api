namespace KittenClaws.Api.Tests.Unit;

using System.Linq;
using KittenClaws.Api.Entities;
using Xunit;

public class FirestoreDocumentTests
{
    [Fact]
    public void ToDocument_OmitsUnsetCreatedByAndUpdatedBy()
    {
        var item = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock", CreatedTimestamp = "2026-01-01T00:00:00.000Z", UpdatedTimestamp = "2026-01-01T00:00:00.000Z" };

        Assert.Equal(new[] { "createdTimestamp", "id", "isDeleted", "name", "updatedTimestamp" }, item.ToDocument().Keys.Order().ToArray());
        item.CreatedBy = "User2";
        Assert.Equal("User2", item.ToDocument()["createdBy"]);
    }
}
