using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Account;

public class UserMongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("user_id")]
    public int UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("full_name")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("is_active")]
    public bool IsActive { get; set; }

    [BsonElement("created_at")]
    [BsonRepresentation(BsonType.DateTime)] // Store as BSON DateTime
    public DateTimeOffset CreatedAt { get; set; }

    [BsonElement("last_login_at")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTimeOffset? LastLoginAt { get; set; }

    [BsonElement("saved_benchmarks_count")]
    public int SavedBenchmarksCount { get; set; }

    [BsonElement("is_etl_user")]
    public bool IsEtlUser { get; set; }
}