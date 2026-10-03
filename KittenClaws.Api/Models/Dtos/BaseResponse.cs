namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class BaseResponse
{
    public BaseResponse()
    {
    }

    public BaseResponse(BaseEntity entity)
    {
        Id = entity.Id;
    }

    // Before the resource's own fields, which System.Text.Json would otherwise write first.
    [JsonPropertyName("id")]
    [JsonPropertyOrder(-1)]
    public string Id { get; set; } = default!;
}
