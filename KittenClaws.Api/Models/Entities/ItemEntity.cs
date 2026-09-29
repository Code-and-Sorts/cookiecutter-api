namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;

public class CatEntity : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}

public class DogEntity : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}
