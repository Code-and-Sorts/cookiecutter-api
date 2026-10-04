namespace KittenClaws.Api.Entities;

using System.Collections.Generic;
using Google.Cloud.Firestore;

[FirestoreData]
public class BaseEntity
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = default!;

    [FirestoreProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [FirestoreProperty("createdTimestamp")]
    public string CreatedTimestamp { get; set; } = default!;

    [FirestoreProperty("updatedTimestamp")]
    public string UpdatedTimestamp { get; set; } = default!;

    [FirestoreProperty("createdBy")]
    public string? CreatedBy { get; set; }

    [FirestoreProperty("updatedBy")]
    public string? UpdatedBy { get; set; }

    public virtual Dictionary<string, object> ToDocument()
    {
        var document = new Dictionary<string, object>
        {
            { "id", Id },
            { "isDeleted", IsDeleted },
            { "createdTimestamp", CreatedTimestamp },
            { "updatedTimestamp", UpdatedTimestamp },
        };
        if (CreatedBy != null)
        {
            document["createdBy"] = CreatedBy;
        }
        if (UpdatedBy != null)
        {
            document["updatedBy"] = UpdatedBy;
        }
        return document;
    }
}
