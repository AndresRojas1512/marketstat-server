using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimStandardJobRoleMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }

    [BsonElement("standard_job_role_title")]
    public string StandardJobRoleTitle { get; set; } = string.Empty;

    [BsonElement("industry_field_id")]
    public int IndustryFieldId { get; set; }
}