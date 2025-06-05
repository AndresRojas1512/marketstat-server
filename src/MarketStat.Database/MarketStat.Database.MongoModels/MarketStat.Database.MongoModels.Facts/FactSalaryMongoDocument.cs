using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Facts;

public class FactSalaryMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("salary_fact_id")]
    public long SalaryFactId { get; set; }

    [BsonElement("date_id")]
    public int DateId { get; set; }

    [BsonElement("city_id")]
    public int CityId { get; set; }

    [BsonElement("employer_id")]
    public int EmployerId { get; set; }

    [BsonElement("job_role_id")]
    public int JobRoleId { get; set; }

    [BsonElement("employee_id")]
    public int EmployeeId { get; set; }

    [BsonElement("salary_amount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SalaryAmount { get; set; }

    [BsonElement("bonus_amount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BonusAmount { get; set; }
}