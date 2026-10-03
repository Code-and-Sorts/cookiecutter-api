namespace KittenClaws.Api.Requests;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class CreateDogRequest : BaseCreateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public DogEntity ToEntity()
    {
        var item = new DogEntity();
        ApplyTo(item);
        item.Name = Name;
        return item;
    }
}

public class ReplaceDogRequest : BaseReplaceRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public void ApplyTo(DogEntity item)
    {
        base.ApplyTo(item);
        item.Name = Name;
    }
}
