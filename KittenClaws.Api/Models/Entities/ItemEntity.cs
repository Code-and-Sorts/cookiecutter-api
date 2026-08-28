namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;

public class KittenClaws : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}
