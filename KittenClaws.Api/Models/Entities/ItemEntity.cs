namespace KittenClaws.Api.Entities;

using Newtonsoft.Json;
using Google.Cloud.Firestore;

[FirestoreData]
public class Cat : BaseEntity
{
    [JsonProperty("name")]
    [FirestoreProperty("name")]
    public string Name { get; set; } = default!;
}

[FirestoreData]
public class Dog : BaseEntity
{
    [JsonProperty("name")]
    [FirestoreProperty("name")]
    public string Name { get; set; } = default!;
}
