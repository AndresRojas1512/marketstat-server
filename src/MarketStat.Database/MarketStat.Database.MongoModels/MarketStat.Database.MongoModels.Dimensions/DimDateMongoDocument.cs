using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimDateMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("date_id")]
    public int DateId { get; set; }

    [BsonElement("full_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
    public DateTime FullDate { get; set; }

    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("quarter")]
    public int Quarter { get; set; }

    [BsonElement("month")]
    public int Month { get; set; }
}