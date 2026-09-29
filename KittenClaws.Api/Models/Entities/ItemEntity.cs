namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;
using Google.Cloud.Firestore;

[FirestoreData]
public class CatEntity : BaseEntity
{
    [JsonProperty("name")]
    [FirestoreProperty("name")]
    public string Name { get; set; } = default!;
}

[FirestoreData]
public class DogEntity : BaseEntity
{
    [JsonProperty("name")]
    [FirestoreProperty("name")]
    public string Name { get; set; } = default!;
}
