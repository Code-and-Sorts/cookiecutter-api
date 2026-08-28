namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;

public class Cat : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}

public class Dog : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}
