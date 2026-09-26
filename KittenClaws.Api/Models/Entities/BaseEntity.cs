namespace KittenClaws.Api.Entities;

using System;
using Newtonsoft.Json;
using Google.Cloud.Firestore;
[FirestoreData]
public class BaseEntity
{
    [JsonProperty("id")]
    [FirestoreProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("isDeleted")]
    [FirestoreProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [JsonProperty("createdTimestamp")]
    [FirestoreProperty("createdTimestamp")]
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedTimestamp")]
    [FirestoreProperty("updatedTimestamp")]
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("createdBy")]
    [FirestoreProperty("createdBy")]
    public string CreatedBy { get; set; } = default!;

    [JsonProperty("updatedBy")]
    [FirestoreProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
