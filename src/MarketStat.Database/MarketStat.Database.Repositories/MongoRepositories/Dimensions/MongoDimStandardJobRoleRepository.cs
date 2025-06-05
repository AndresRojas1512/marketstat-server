using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimStandardJobRoleRepository : IDimStandardJobRoleRepository
{
    private readonly IMongoCollection<DimStandardJobRoleMongoDocument> _standardJobRolesCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimStandardJobRoleRepository> _logger;

    public MongoDimStandardJobRoleRepository(IMongoDatabase database, ILogger<MongoDimStandardJobRoleRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _standardJobRolesCollection = database.GetCollection<DimStandardJobRoleMongoDocument>("dim_standard_job_roles");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var idIndex = Builders<DimStandardJobRoleMongoDocument>.IndexKeys.Ascending(x => x.StandardJobRoleId);
        await _standardJobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimStandardJobRoleMongoDocument>(idIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_standard_job_role_id_unique" })
        );

        var titleIndex = Builders<DimStandardJobRoleMongoDocument>.IndexKeys.Ascending(x => x.StandardJobRoleTitle);
        await _standardJobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimStandardJobRoleMongoDocument>(titleIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_standard_job_role_title_unique" })
        );

        var industryFieldIdIndex = Builders<DimStandardJobRoleMongoDocument>.IndexKeys.Ascending(x => x.IndustryFieldId);
        await _standardJobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimStandardJobRoleMongoDocument>(industryFieldIdIndex, 
            new CreateIndexOptions { Name = "idx_sjr_industry_field_id" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_standard_job_roles' collection.");
    }

    private DimStandardJobRole ToDomain(DimStandardJobRoleMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimStandardJobRole
        {
            StandardJobRoleId = doc.StandardJobRoleId,
            StandardJobRoleTitle = doc.StandardJobRoleTitle,
            IndustryFieldId = doc.IndustryFieldId
        };
    }

    private DimStandardJobRoleMongoDocument FromDomain(DimStandardJobRole domain)
    {
        if (domain == null) return null!;
        return new DimStandardJobRoleMongoDocument
        {
            StandardJobRoleId = domain.StandardJobRoleId,
            StandardJobRoleTitle = domain.StandardJobRoleTitle,
            IndustryFieldId = domain.IndustryFieldId
        };
    }
    
    public async Task AddStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        _logger.LogInformation("MongoRepo: Attempting to add standard job role: {Title}", jobRole.StandardJobRoleTitle);
        if (jobRole.StandardJobRoleId == 0) 
        {
            jobRole.StandardJobRoleId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "standard_job_role_id");
            _logger.LogInformation("MongoRepo: Generated new StandardJobRoleId {Id} for {Title}", 
                                   jobRole.StandardJobRoleId, jobRole.StandardJobRoleTitle);
        }

        var document = FromDomain(jobRole);
        
        try
        {
            await _standardJobRolesCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Standard job role '{Title}' added with ID {Id}, ObjectId {MongoId}", 
                                   document.StandardJobRoleTitle, document.StandardJobRoleId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding standard job role '{Title}'.", jobRole.StandardJobRoleTitle);
            string errorMessage = $"A standard job role with the title '{jobRole.StandardJobRoleTitle}' or ID {jobRole.StandardJobRoleId} already exists.";
            if (mwx.Message.Contains("idx_standard_job_role_title_unique")) errorMessage = $"A standard job role titled '{jobRole.StandardJobRoleTitle}' already exists.";
            else if (mwx.Message.Contains("idx_standard_job_role_id_unique")) errorMessage = $"StandardJobRoleId {jobRole.StandardJobRoleId} conflict.";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id)
    {
        _logger.LogDebug("MongoRepo: Getting standard job role by StandardJobRoleId: {Id}", id);
        var filter = Builders<DimStandardJobRoleMongoDocument>.Filter.Eq(doc => doc.StandardJobRoleId, id);
        var document = await _standardJobRolesCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Standard job role with ID {Id} not found.", id);
            throw new NotFoundException($"Standard job role with ID {id} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all standard job roles.");
        var documents = await _standardJobRolesCollection.Find(FilterDefinition<DimStandardJobRoleMongoDocument>.Empty)
                                                       .Sort(Builders<DimStandardJobRoleMongoDocument>.Sort.Ascending(x => x.StandardJobRoleTitle))
                                                       .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetStandardJobRolesByIndustryAsync(int industryFieldId)
    {
        _logger.LogDebug("MongoRepo: Getting standard job roles by IndustryFieldId: {IndustryFieldId}", industryFieldId);
         if (industryFieldId <= 0)
        {
            _logger.LogWarning("MongoRepo: GetStandardJobRolesByIndustryAsync called with invalid IndustryFieldId: {IndustryFieldId}", industryFieldId);
            return Enumerable.Empty<DimStandardJobRole>();
        }
        var filter = Builders<DimStandardJobRoleMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryFieldId);
        var documents = await _standardJobRolesCollection.Find(filter)
                                                         .Sort(Builders<DimStandardJobRoleMongoDocument>.Sort.Ascending(x => x.StandardJobRoleTitle))
                                                         .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        _logger.LogInformation("MongoRepo: Attempting to update standard job role with ID: {Id}", jobRole.StandardJobRoleId);
        var filter = Builders<DimStandardJobRoleMongoDocument>.Filter.Eq(doc => doc.StandardJobRoleId, jobRole.StandardJobRoleId);
        
        var existingDocument = await _standardJobRolesCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            _logger.LogWarning("MongoRepo: StandardJobRoleId {Id} not found for update.", jobRole.StandardJobRoleId);
            throw new NotFoundException($"Standard job role with ID {jobRole.StandardJobRoleId} not found for update.");
        }
        
        existingDocument.StandardJobRoleTitle = jobRole.StandardJobRoleTitle;
        existingDocument.IndustryFieldId = jobRole.IndustryFieldId;

        try
        {
            var result = await _standardJobRolesCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: StandardJobRoleId {Id} not matched during ReplaceOne.", jobRole.StandardJobRoleId);
                throw new NotFoundException($"Standard job role with ID {jobRole.StandardJobRoleId} not found for update (concurrent modification?).");
            }
            _logger.LogInformation("MongoRepo: Standard job role with ID {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   jobRole.StandardJobRoleId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating standard job role ID {Id} to title '{Title}'.", 
                               jobRole.StandardJobRoleId, jobRole.StandardJobRoleTitle);
            throw new ConflictException($"A standard job role titled '{jobRole.StandardJobRoleTitle}' already exists.");
        }
    }

    public async Task DeleteStandardJobRoleAsync(int id)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete standard job role with ID: {Id}", id);
        var filter = Builders<DimStandardJobRoleMongoDocument>.Filter.Eq(doc => doc.StandardJobRoleId, id);
        var result = await _standardJobRolesCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Standard job role with ID {Id} not found for deletion.", id);
            throw new NotFoundException($"Standard job role with ID {id} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Standard job role with ID {Id} deleted. Count: {DeletedCount}", 
                               id, result.DeletedCount);
    }
}