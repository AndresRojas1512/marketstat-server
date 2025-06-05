using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimHierarchyLevelMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }

    [BsonElement("hierarchy_level_name")]
    public string HierarchyLevelName { get; set; } = string.Empty;
}