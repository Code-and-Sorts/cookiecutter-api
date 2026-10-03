namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class DogDto : BaseResponse
{
    public DogDto()
    {
    }

    public DogDto(DogEntity item)
        : base(item)
    {
        Name = item.Name;
    }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Name { get; set; }
}
