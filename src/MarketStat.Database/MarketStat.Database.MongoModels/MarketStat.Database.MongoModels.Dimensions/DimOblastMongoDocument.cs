using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimOblastMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("oblast_id")]
    public int OblastId { get; set; }

    [BsonElement("oblast_name")]
    public string OblastName { get; set; } = string.Empty;

    [BsonElement("district_id")]
    public int DistrictId { get; set; }
}