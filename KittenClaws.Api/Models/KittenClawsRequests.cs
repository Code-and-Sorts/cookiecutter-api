namespace KittenClaws.Api.Requests;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class CreateKittenClawsRequest : BaseCreateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public KittenClawsEntity ToEntity()
    {
        var item = new KittenClawsEntity();
        ApplyTo(item);
        item.Name = Name;
        return item;
    }
}

public class UpdateKittenClawsRequest : BaseUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public void ApplyTo(KittenClawsEntity item)
    {
        base.ApplyTo(item);
        if (Sent.Contains("name"))
        {
            item.Name = Name;
        }
    }
}
