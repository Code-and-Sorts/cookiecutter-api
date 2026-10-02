namespace KittenClaws.Api.Entities;

public class BaseEntity
{
    public string Id { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;

    public string CreatedTimestamp { get; set; } = default!;

    public string UpdatedTimestamp { get; set; } = default!;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
