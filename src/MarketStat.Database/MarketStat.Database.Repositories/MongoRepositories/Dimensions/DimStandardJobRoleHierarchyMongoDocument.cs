using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class DimStandardJobRoleHierarchyMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }

    [BsonElement("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }
}