namespace KittenClaws.Api.Entities;

using Google.Cloud.Firestore;

[FirestoreData]
public class DogEntity : BaseEntity
{
    [FirestoreProperty("name")]
    public string Name { get; set; } = default!;

    public override Dictionary<string, object> ToDocument()
    {
        var document = base.ToDocument();
        document["name"] = Name;
        return document;
    }
}
