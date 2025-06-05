using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;

public class DimEmployeeEducationMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("employee_id")]
    public int EmployeeId { get; set; }

    [BsonElement("education_id")]
    public int EducationId { get; set; }

    [BsonElement("graduation_year")]
    public short GraduationYear { get; set; }
}