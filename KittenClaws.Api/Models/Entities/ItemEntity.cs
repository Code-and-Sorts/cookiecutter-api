namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;
using Google.Cloud.Firestore;

[FirestoreData]
public class KittenClaws : BaseEntity
{
    [JsonProperty("name")]
    [FirestoreProperty("name")]
    public string Name { get; set; } = default!;
}
