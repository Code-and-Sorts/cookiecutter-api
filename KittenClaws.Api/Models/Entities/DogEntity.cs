namespace KittenClaws.Api.Entities;

using Amazon.DynamoDBv2.Model;

public class DogEntity : BaseEntity
{
    public string Name { get; set; } = default!;

    public override void WriteAttributes(Dictionary<string, AttributeValue> item) => item["name"] = new AttributeValue { S = Name };

    public override void ReadAttributes(Dictionary<string, AttributeValue> item) =>
        Name = item.TryGetValue("name", out var name) ? name.S : string.Empty;
}
