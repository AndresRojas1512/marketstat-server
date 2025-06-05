using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimFederalDistrictMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("district_id")]
    public int DistrictId { get; set; }

    [BsonElement("district_name")]
    public string DistrictName { get; set; } = string.Empty;
}