using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimEducationMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("education_id")]
    public int EducationId { get; set; }

    [BsonElement("specialty")]
    public string Specialty { get; set; } = string.Empty;

    [BsonElement("specialty_code")]
    public string SpecialtyCode { get; set; } = string.Empty;

    [BsonElement("education_level_id")]
    public int EducationLevelId { get; set; }
}