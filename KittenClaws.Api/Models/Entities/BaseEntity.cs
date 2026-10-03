namespace KittenClaws.Api.Entities;

using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

public class BaseEntity
{
    public string Id { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;

    public string CreatedTimestamp { get; set; } = default!;

    public string UpdatedTimestamp { get; set; } = default!;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual void WriteAttributes(Dictionary<string, AttributeValue> item)
    {
    }

    public virtual void ReadAttributes(Dictionary<string, AttributeValue> item)
    {
    }
}
