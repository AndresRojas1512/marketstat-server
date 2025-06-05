using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Account;

public class BenchmarkHistoryMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("benchmark_history_id")]
    public long BenchmarkHistoryId { get; set; }

    [BsonElement("user_id")]
    public int UserId { get; set; }

    [BsonElement("benchmark_name")]
    public string? BenchmarkName { get; set; }

    [BsonElement("saved_at")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTimeOffset SavedAt { get; set; }

    [BsonElement("filter_industry_field_id")]
    public int? FilterIndustryFieldId { get; set; }

    [BsonElement("filter_standard_job_role_id")]
    public int? FilterStandardJobRoleId { get; set; }

    [BsonElement("filter_hierarchy_level_id")]
    public int? FilterHierarchyLevelId { get; set; }

    [BsonElement("filter_district_id")]
    public int? FilterDistrictId { get; set; }

    [BsonElement("filter_oblast_id")]
    public int? FilterOblastId { get; set; }

    [BsonElement("filter_city_id")]
    public int? FilterCityId { get; set; }

    [BsonElement("filter_date_start")]
    [BsonRepresentation(BsonType.String)]
    public string? FilterDateStart { get; set; }

    [BsonElement("filter_date_end")]
    [BsonRepresentation(BsonType.String)]
    public string? FilterDateEnd { get; set; }

    [BsonElement("filter_target_percentile")]
    public int? FilterTargetPercentile { get; set; }

    [BsonElement("filter_granularity")]
    public string? FilterGranularity { get; set; }

    [BsonElement("filter_periods")]
    public int? FilterPeriods { get; set; }

    [BsonElement("benchmark_result_json")]
    public string BenchmarkResultJson { get; set; } = string.Empty;
}