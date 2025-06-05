using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimJobRoleMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("job_role_id")]
    public int JobRoleId { get; set; }

    [BsonElement("job_role_title")]
    public string JobRoleTitle { get; set; } = string.Empty;

    [BsonElement("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }

    [BsonElement("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }
}