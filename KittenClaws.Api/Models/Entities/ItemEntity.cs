namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;

public class KittenClawsEntity : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}
