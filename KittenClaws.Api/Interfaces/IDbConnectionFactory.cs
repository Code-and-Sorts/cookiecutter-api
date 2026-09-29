
namespace KittenClaws.Api.Interfaces;

using Google.Cloud.Firestore;

public interface IDbConnectionFactory
{
    FirestoreDb CreateClient();
}
