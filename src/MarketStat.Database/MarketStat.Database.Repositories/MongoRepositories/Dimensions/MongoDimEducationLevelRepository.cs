using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimEducationLevelRepository : IDimEducationLevelRepository
{
    private readonly IMongoCollection<DimEducationLevelMongoDocument> _educationLevelsCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimEducationLevelRepository> _logger;

    public MongoDimEducationLevelRepository(IMongoDatabase database, ILogger<MongoDimEducationLevelRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _educationLevelsCollection = database.GetCollection<DimEducationLevelMongoDocument>("dim_education_levels");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var educationLevelIdIndex = Builders<DimEducationLevelMongoDocument>.IndexKeys.Ascending(x => x.EducationLevelId);
        await _educationLevelsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEducationLevelMongoDocument>(educationLevelIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_education_level_id_unique" })
        );

        var nameIndex = Builders<DimEducationLevelMongoDocument>.IndexKeys.Ascending(x => x.EducationLevelName);
        await _educationLevelsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEducationLevelMongoDocument>(nameIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_education_level_name_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_education_levels' collection.");
    }

    private DimEducationLevel ToDomain(DimEducationLevelMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimEducationLevel
        {
            EducationLevelId = doc.EducationLevelId,
            EducationLevelName = doc.EducationLevelName
        };
    }

    private DimEducationLevelMongoDocument FromDomain(DimEducationLevel domain)
    {
        if (domain == null) return null!;
        return new DimEducationLevelMongoDocument
        {
            EducationLevelId = domain.EducationLevelId,
            EducationLevelName = domain.EducationLevelName
        };
    }

    public async Task AddEducationLevelAsync(DimEducationLevel educationLevel)
    {
        _logger.LogInformation("MongoRepo: Attempting to add education level: {EducationLevelName}", educationLevel.EducationLevelName);
        if (educationLevel.EducationLevelId == 0)
        {
            educationLevel.EducationLevelId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "education_level_id");
            _logger.LogInformation("MongoRepo: Generated new EducationLevelId {EducationLevelId} for {EducationLevelName}", 
                                   educationLevel.EducationLevelId, educationLevel.EducationLevelName);
        }

        var document = FromDomain(educationLevel);
        
        try
        {
            await _educationLevelsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Education level '{EducationLevelName}' added with EducationLevelId {EducationLevelId}, ObjectId {ObjectId}", 
                                   document.EducationLevelName, document.EducationLevelId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding education level '{EducationLevelName}'. It might already exist (name or EducationLevelId).", educationLevel.EducationLevelName);
            string errorMessage = $"An education level with the name '{educationLevel.EducationLevelName}' or ID {educationLevel.EducationLevelId} already exists.";
            if(mwx.Message.Contains("idx_education_level_name_unique")) errorMessage = $"An education level named '{educationLevel.EducationLevelName}' already exists.";
            else if (mwx.Message.Contains("idx_education_level_id_unique")) errorMessage = $"EducationLevelId {educationLevel.EducationLevelId} conflict (should not happen with sequence).";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimEducationLevel> GetEducationLevelByIdAsync(int id)
    {
        _logger.LogDebug("MongoRepo: Getting education level by EducationLevelId: {Id}", id);
        var filter = Builders<DimEducationLevelMongoDocument>.Filter.Eq(doc => doc.EducationLevelId, id);
        var document = await _educationLevelsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Education level with EducationLevelId {Id} not found.", id);
            throw new NotFoundException($"Education level with ID {id} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimEducationLevel>> GetAllEducationLevelsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all education levels.");
        var documents = await _educationLevelsCollection.Find(FilterDefinition<DimEducationLevelMongoDocument>.Empty)
                                                       .Sort(Builders<DimEducationLevelMongoDocument>.Sort.Ascending(x => x.EducationLevelName))
                                                       .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateEducationLevelAsync(DimEducationLevel educationLevel)
    {
        _logger.LogInformation("MongoRepo: Attempting to update education level with EducationLevelId: {Id}", educationLevel.EducationLevelId);
        var filter = Builders<DimEducationLevelMongoDocument>.Filter.Eq(doc => doc.EducationLevelId, educationLevel.EducationLevelId);
        
        var documentToUpdate = FromDomain(educationLevel);

        var existingDocument = await _educationLevelsCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
             _logger.LogWarning("MongoRepo: EducationLevelId {Id} not found for update.", educationLevel.EducationLevelId);
            throw new NotFoundException($"Education level with ID {educationLevel.EducationLevelId} not found for update.");
        }
        
        existingDocument.EducationLevelName = educationLevel.EducationLevelName;

        try
        {
            var result = await _educationLevelsCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: EducationLevelId {Id} not found during ReplaceOne operation.", educationLevel.EducationLevelId);
                throw new NotFoundException($"Education level with ID {educationLevel.EducationLevelId} not found for update (concurrent modification?).");
            }
            _logger.LogInformation("MongoRepo: Education level with EducationLevelId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   educationLevel.EducationLevelId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating education level EducationLevelId {Id} to name '{Name}'.", 
                               educationLevel.EducationLevelId, educationLevel.EducationLevelName);
            throw new ConflictException($"An education level named '{educationLevel.EducationLevelName}' already exists.");
        }
    }

    public async Task DeleteEducationLevelAsync(int id)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete education level with EducationLevelId: {Id}", id);
        var filter = Builders<DimEducationLevelMongoDocument>.Filter.Eq(doc => doc.EducationLevelId, id);
        var result = await _educationLevelsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Education level with EducationLevelId {Id} not found for deletion.", id);
            throw new NotFoundException($"Education level with ID {id} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Education level with EducationLevelId {Id} deleted. Count: {DeletedCount}", id, result.DeletedCount);
    }
}