using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimEmployeeMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("employee_id")]
    public int EmployeeId { get; set; }

    [BsonElement("birth_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
    public DateTime BirthDate { get; set; }

    [BsonElement("career_start_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
    public DateTime CareerStartDate { get; set; }
}