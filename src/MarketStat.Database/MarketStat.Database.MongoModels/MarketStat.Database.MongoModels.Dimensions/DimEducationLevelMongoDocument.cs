using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimEducationLevelMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("education_level_id")]
    public int EducationLevelId { get; set; }

    [BsonElement("education_level_name")]
    public string EducationLevelName { get; set; } = string.Empty;
}