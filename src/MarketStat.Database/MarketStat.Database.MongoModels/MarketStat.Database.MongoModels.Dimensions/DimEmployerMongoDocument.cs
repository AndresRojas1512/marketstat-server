using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimEmployerMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("employer_id")]
    public int EmployerId { get; set; }

    [BsonElement("employer_name")]
    public string EmployerName { get; set; } = string.Empty;

    [BsonElement("inn")]
    public string Inn { get; set; } = string.Empty;

    [BsonElement("ogrn")]
    public string Ogrn { get; set; } = string.Empty;

    [BsonElement("kpp")]
    public string Kpp { get; set; } = string.Empty;

    [BsonElement("registration_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
    public DateTime RegistrationDate { get; set; }

    [BsonElement("legal_address")]
    public string LegalAddress { get; set; } = string.Empty;

    [BsonElement("website")]
    public string Website { get; set; } = string.Empty;

    [BsonElement("contact_email")]
    public string ContactEmail { get; set; } = string.Empty;

    [BsonElement("contact_phone")]
    public string ContactPhone { get; set; } = string.Empty;
        
}