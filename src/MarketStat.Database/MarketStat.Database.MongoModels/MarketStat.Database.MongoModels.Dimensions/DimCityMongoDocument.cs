using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimCityMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("city_id")]
    public int CityId { get; set; }

    [BsonElement("city_name")]
    public string CityName { get; set; } = string.Empty;

    [BsonElement("oblast_id")]
    public int OblastId { get; set; } 

    [BsonElement("oblast_name")]
    public string? OblastName { get; set; } 
}