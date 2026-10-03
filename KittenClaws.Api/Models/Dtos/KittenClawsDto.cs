namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class KittenClawsDto : BaseResponse
{
    public KittenClawsDto()
    {
    }

    public KittenClawsDto(KittenClawsEntity item)
        : base(item)
    {
        Name = item.Name;
    }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Name { get; set; }
}
