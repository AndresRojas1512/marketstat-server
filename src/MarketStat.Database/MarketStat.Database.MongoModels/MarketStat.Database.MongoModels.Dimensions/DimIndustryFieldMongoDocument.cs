using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimIndustryFieldMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("industry_field_id")]
    public int IndustryFieldId { get; set; }

    [BsonElement("industry_field_name")]
    public string IndustryFieldName { get; set; } = string.Empty;
}