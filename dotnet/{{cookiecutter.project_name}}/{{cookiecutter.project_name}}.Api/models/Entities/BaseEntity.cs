namespace {{cookiecutter.project_name}}.Api.Entities;

using System;

public class BaseEntity
{
    public string Id { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;

    public string CreatedBy { get; set; } = default!;

    public string UpdatedBy { get; set; } = default!;
}
