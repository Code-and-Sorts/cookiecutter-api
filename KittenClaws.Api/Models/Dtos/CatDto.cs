namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class CatDto : BaseResponse
{
    public CatDto()
    {
    }

    public CatDto(CatEntity item)
        : base(item)
    {
        Name = item.Name;
    }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Name { get; set; }
}
