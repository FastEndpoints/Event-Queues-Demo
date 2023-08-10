using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Entities;

namespace PublisherServer;

public class DbContext : DBContext
{
    public DbContext(string database, string host) : base(database, host)
    {
        var objectSerializer = new ObjectSerializer(type =>
        ObjectSerializer.DefaultAllowedTypes(type) ||
        type.FullName!.StartsWith("Contracts"));

        BsonSerializer.TryRegisterSerializer(objectSerializer);
    }
}