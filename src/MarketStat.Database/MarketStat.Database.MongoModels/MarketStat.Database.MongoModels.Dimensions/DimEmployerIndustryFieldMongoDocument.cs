using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimEmployerIndustryFieldMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("employer_id")]
    public int EmployerId { get; set; }

    [BsonElement("industry_field_id")]
    public int IndustryFieldId { get; set; }
}