namespace KittenClaws.Api.Requests;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class CreateCatRequest : BaseCreateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public CatEntity ToEntity()
    {
        var item = new CatEntity();
        ApplyTo(item);
        item.Name = Name;
        return item;
    }
}

public class UpdateCatRequest : BaseUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public void ApplyTo(CatEntity item)
    {
        base.ApplyTo(item);
        if (Sent.Contains("name"))
        {
            item.Name = Name;
        }
    }
}
