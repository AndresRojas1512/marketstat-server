using System.Text.RegularExpressions;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Account;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Account;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserMongoDocument> _usersCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoUserRepository> _logger;

    // We'll need a converter or manual mapping logic.
    // For simplicity, manual mapping is done here.
    // In a larger app, a UserMongoConverter class would be better.

    public MongoUserRepository(IMongoDatabase database, ILogger<MongoUserRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _usersCollection = database.GetCollection<UserMongoDocument>("users");
        _countersCollection = database.GetCollection<CounterDocument>("counters");

        // Consider calling an EnsureIndexesAsync method here or at startup
        // CreateIndexesAsync().GetAwaiter().GetResult(); 
    }

    public async Task CreateIndexesAsync()
    {
        var userIdIndex = Builders<UserMongoDocument>.IndexKeys.Ascending(x => x.UserId);
        await _usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongoDocument>(userIdIndex, new CreateIndexOptions { Unique = true, Name = "idx_user_id_unique" })
        );

        var usernameIndex = Builders<UserMongoDocument>.IndexKeys.Ascending(x => x.Username);
        await _usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongoDocument>(usernameIndex, new CreateIndexOptions { Unique = true, Name = "idx_username_unique" })
        );

        var emailIndex = Builders<UserMongoDocument>.IndexKeys.Ascending(x => x.Email);
        await _usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongoDocument>(emailIndex, new CreateIndexOptions { Unique = true, Name = "idx_email_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'users' collection.");
    }

    private User ToDomain(UserMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new User
        {
            UserId = doc.UserId,
            Username = doc.Username,
            PasswordHash = doc.PasswordHash,
            Email = doc.Email,
            FullName = doc.FullName,
            IsActive = doc.IsActive,
            CreatedAt = doc.CreatedAt,
            LastLoginAt = doc.LastLoginAt,
            SavedBenchmarksCount = doc.SavedBenchmarksCount,
            IsEtlUser = doc.IsEtlUser
        };
    }

    private UserMongoDocument FromDomain(User domainUser)
    {
        if (domainUser == null) return null!;
        return new UserMongoDocument
        {
            UserId = domainUser.UserId,
            Username = domainUser.Username,
            PasswordHash = domainUser.PasswordHash,
            Email = domainUser.Email,
            FullName = domainUser.FullName,
            IsActive = domainUser.IsActive,
            CreatedAt = domainUser.CreatedAt,
            LastLoginAt = domainUser.LastLoginAt,
            SavedBenchmarksCount = domainUser.SavedBenchmarksCount,
            IsEtlUser = domainUser.IsEtlUser
        };
    }

    public async Task<User> AddUserAsync(User user)
    {
        _logger.LogInformation("MongoRepo: Attempting to add user: {Username}", user.Username);
        if (user.UserId == 0)
        {
            user.UserId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "user_id");
            _logger.LogInformation("MongoRepo: Generated new UserId {UserId} for {Username}", user.UserId, user.Username);
        }

        var document = FromDomain(user);
        
        try
        {
            await _usersCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: User '{Username}' added with UserId {UserId}, ObjectId {ObjectId}", 
                                   document.Username, document.UserId, document.Id);
            return ToDomain(document);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding user '{Username}'. It might already exist (username, email, or UserId).", user.Username);
            string errorMessage = "A user with the same identity (username, email, or user ID) already exists.";
            if (mwx.Message.Contains("idx_username_unique")) errorMessage = "Username already exists.";
            else if (mwx.Message.Contains("idx_email_unique")) errorMessage = "Email already exists.";
            else if (mwx.Message.Contains("idx_user_id_unique")) errorMessage = "User ID conflict (should not happen with sequence).";
            
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        _logger.LogDebug("MongoRepo: Getting user by Username: {Username}", username);
        var filter = Builders<UserMongoDocument>.Filter.Eq(doc => doc.Username, username);
        var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: User with Username '{Username}' not found.", username);
            throw new NotFoundException($"User with username '{username}' not found.");
        }
        return ToDomain(document);
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        _logger.LogDebug("MongoRepo: Getting user by UserId: {UserId}", userId);
        var filter = Builders<UserMongoDocument>.Filter.Eq(doc => doc.UserId, userId);
        var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();

        if (document == null)
        {
            _logger.LogWarning("MongoRepo: User with UserId {UserId} not found.", userId);
            throw new NotFoundException($"User with ID {userId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        _logger.LogDebug("MongoRepo: Checking if user exists by Username: {Username} or Email: {Email}", username, email);
        var filter = Builders<UserMongoDocument>.Filter.Or(
            Builders<UserMongoDocument>.Filter.Regex(doc => doc.Username, new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")),
            Builders<UserMongoDocument>.Filter.Regex(doc => doc.Email, new BsonRegularExpression($"^{Regex.Escape(email)}$", "i"))
        );
        // Simpler case-sensitive check:
        // var filter = Builders<UserMongoDocument>.Filter.Or(
        // Builders<UserMongoDocument>.Filter.Eq(doc => doc.Username, username),
        // Builders<UserMongoDocument>.Filter.Eq(doc => doc.Email, email)
        // );
        var count = await _usersCollection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task UpdateUserAsync(User user)
    {
        _logger.LogInformation("MongoRepo: Attempting to update user with UserId: {UserId}", user.UserId);
        var filter = Builders<UserMongoDocument>.Filter.Eq(doc => doc.UserId, user.UserId);
        
        var updateDefinition = Builders<UserMongoDocument>.Update
            .Set(doc => doc.FullName, user.FullName)
            .Set(doc => doc.Email, user.Email)
            .Set(doc => doc.IsActive, user.IsActive)
            .Set(doc => doc.LastLoginAt, user.LastLoginAt)
            .Set(doc => doc.SavedBenchmarksCount, user.SavedBenchmarksCount);

        try
        {
            var result = await _usersCollection.UpdateOneAsync(filter, updateDefinition);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: User with UserId {UserId} not found for update.", user.UserId);
                throw new NotFoundException($"User with ID {user.UserId} not found for update.");
            }
            _logger.LogInformation("MongoRepo: User with UserId {UserId} updated. Matched: {Matched}, Modified: {Modified}", 
                                   user.UserId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
             _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating user UserId {UserId} (likely email conflict).", user.UserId);
            throw new ConflictException("Update failed due to a conflict (e.g., email already taken by another user).");
        }
    }
}