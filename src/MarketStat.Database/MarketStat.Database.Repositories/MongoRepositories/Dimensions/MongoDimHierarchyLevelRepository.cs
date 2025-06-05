using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimHierarchyLevelRepository : IDimHierarchyLevelRepository
{
    private readonly IMongoCollection<DimHierarchyLevelMongoDocument> _hierarchyLevelsCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimHierarchyLevelRepository> _logger;

    public MongoDimHierarchyLevelRepository(IMongoDatabase database, ILogger<MongoDimHierarchyLevelRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _hierarchyLevelsCollection = database.GetCollection<DimHierarchyLevelMongoDocument>("dim_hierarchy_levels");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var hierarchyLevelIdIndex = Builders<DimHierarchyLevelMongoDocument>.IndexKeys.Ascending(x => x.HierarchyLevelId);
        await _hierarchyLevelsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimHierarchyLevelMongoDocument>(hierarchyLevelIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_hierarchy_level_id_unique" })
        );
        
        var nameIndex = Builders<DimHierarchyLevelMongoDocument>.IndexKeys.Ascending(x => x.HierarchyLevelName);
        await _hierarchyLevelsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimHierarchyLevelMongoDocument>(nameIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_hierarchy_level_name_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_hierarchy_levels' collection.");
    }

    private DimHierarchyLevel ToDomain(DimHierarchyLevelMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimHierarchyLevel
        {
            HierarchyLevelId = doc.HierarchyLevelId,
            HierarchyLevelName = doc.HierarchyLevelName
        };
    }

    private DimHierarchyLevelMongoDocument FromDomain(DimHierarchyLevel domain)
    {
        if (domain == null) return null!;
        return new DimHierarchyLevelMongoDocument
        {
            HierarchyLevelId = domain.HierarchyLevelId,
            HierarchyLevelName = domain.HierarchyLevelName
        };
    }
    
    public async Task AddHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel)
    {
        _logger.LogInformation("MongoRepo: Attempting to add hierarchy level: {HierarchyLevelName}", dimHierarchyLevel.HierarchyLevelName);
        if (dimHierarchyLevel.HierarchyLevelId == 0) // New level, generate ID
        {
            dimHierarchyLevel.HierarchyLevelId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "hierarchy_level_id");
            _logger.LogInformation("MongoRepo: Generated new HierarchyLevelId {HierarchyLevelId} for {HierarchyLevelName}", 
                                   dimHierarchyLevel.HierarchyLevelId, dimHierarchyLevel.HierarchyLevelName);
        }

        var document = FromDomain(dimHierarchyLevel);
        
        try
        {
            await _hierarchyLevelsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Hierarchy level '{HierarchyLevelName}' added with HierarchyLevelId {HierarchyLevelId}, ObjectId {ObjectId}", 
                                   document.HierarchyLevelName, document.HierarchyLevelId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding hierarchy level '{HierarchyLevelName}'. It might already exist (name or HierarchyLevelId).", dimHierarchyLevel.HierarchyLevelName);
            string errorMessage = $"A hierarchy level with the name '{dimHierarchyLevel.HierarchyLevelName}' or ID {dimHierarchyLevel.HierarchyLevelId} already exists.";
             if(mwx.Message.Contains("idx_hierarchy_level_name_unique")) errorMessage = $"A hierarchy level named '{dimHierarchyLevel.HierarchyLevelName}' already exists.";
             else if (mwx.Message.Contains("idx_hierarchy_level_id_unique")) errorMessage = $"HierarchyLevelId {dimHierarchyLevel.HierarchyLevelId} conflict.";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id)
    {
        _logger.LogDebug("MongoRepo: Getting hierarchy level by HierarchyLevelId: {Id}", id);
        var filter = Builders<DimHierarchyLevelMongoDocument>.Filter.Eq(doc => doc.HierarchyLevelId, id);
        var document = await _hierarchyLevelsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Hierarchy level with HierarchyLevelId {Id} not found.", id);
            throw new NotFoundException($"Hierarchy level with ID {id} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimHierarchyLevel>> GetAllHierarchyLevelsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all hierarchy levels.");
        var documents = await _hierarchyLevelsCollection.Find(FilterDefinition<DimHierarchyLevelMongoDocument>.Empty)
                                                       .Sort(Builders<DimHierarchyLevelMongoDocument>.Sort.Ascending(x => x.HierarchyLevelName))
                                                       .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel)
    {
        _logger.LogInformation("MongoRepo: Attempting to update hierarchy level with HierarchyLevelId: {Id}", dimHierarchyLevel.HierarchyLevelId);
        var filter = Builders<DimHierarchyLevelMongoDocument>.Filter.Eq(doc => doc.HierarchyLevelId, dimHierarchyLevel.HierarchyLevelId);
        
        var existingDocument = await _hierarchyLevelsCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
             _logger.LogWarning("MongoRepo: HierarchyLevelId {Id} not found for update.", dimHierarchyLevel.HierarchyLevelId);
            throw new NotFoundException($"Hierarchy level with ID {dimHierarchyLevel.HierarchyLevelId} not found for update.");
        }
        
        existingDocument.HierarchyLevelName = dimHierarchyLevel.HierarchyLevelName;

        try
        {
            var result = await _hierarchyLevelsCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                 _logger.LogWarning("MongoRepo: HierarchyLevelId {Id} not matched during ReplaceOne operation.", dimHierarchyLevel.HierarchyLevelId);
                throw new NotFoundException($"Hierarchy level with ID {dimHierarchyLevel.HierarchyLevelId} not found for update (concurrent modification?).");
            }
             _logger.LogInformation("MongoRepo: Hierarchy level with HierarchyLevelId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   dimHierarchyLevel.HierarchyLevelId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating hierarchy level HierarchyLevelId {Id} to name '{Name}'.", 
                               dimHierarchyLevel.HierarchyLevelId, dimHierarchyLevel.HierarchyLevelName);
            throw new ConflictException($"A hierarchy level named '{dimHierarchyLevel.HierarchyLevelName}' already exists.");
        }
    }

    public async Task DeleteHierarchyLevelAsync(int id)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete hierarchy level with HierarchyLevelId: {Id}", id);
        var filter = Builders<DimHierarchyLevelMongoDocument>.Filter.Eq(doc => doc.HierarchyLevelId, id);
        var result = await _hierarchyLevelsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
             _logger.LogWarning("MongoRepo: Hierarchy level with HierarchyLevelId {Id} not found for deletion.", id);
            throw new NotFoundException($"Hierarchy level with ID {id} not found for deletion.");
        }
         _logger.LogInformation("MongoRepo: Hierarchy level with HierarchyLevelId {Id} deleted. Count: {DeletedCount}", 
                               id, result.DeletedCount);
    }
}