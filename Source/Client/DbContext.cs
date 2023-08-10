using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Entities;

namespace SubscriberClient;

public class DbContext : DBContext
{
    public DbContext(string database, string host) : base(database, host)
    {
        try
        {
            var objectSerializer = new ObjectSerializer(type =>
            ObjectSerializer.DefaultAllowedTypes(type) ||
            type.FullName!.StartsWith("Contracts"));

            BsonSerializer.RegisterSerializer(objectSerializer);
        }
        catch { }
    }
}