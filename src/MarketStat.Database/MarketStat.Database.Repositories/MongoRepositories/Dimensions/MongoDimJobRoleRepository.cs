using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimJobRoleRepository : IDimJobRoleRepository
{
    private readonly IMongoCollection<DimJobRoleMongoDocument> _jobRolesCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimJobRoleRepository> _logger;

    public MongoDimJobRoleRepository(IMongoDatabase database, ILogger<MongoDimJobRoleRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _jobRolesCollection = database.GetCollection<DimJobRoleMongoDocument>("dim_job_roles");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var jobRoleIdIndex = Builders<DimJobRoleMongoDocument>.IndexKeys.Ascending(x => x.JobRoleId);
        await _jobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimJobRoleMongoDocument>(jobRoleIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_job_role_id_unique" })
        );

        var naturalKeyIndex = Builders<DimJobRoleMongoDocument>.IndexKeys
            .Ascending(x => x.JobRoleTitle)
            .Ascending(x => x.StandardJobRoleId)
            .Ascending(x => x.HierarchyLevelId);
        await _jobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimJobRoleMongoDocument>(naturalKeyIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_job_role_natural_key_unique" })
        );

        var standardJobRoleIdIndex = Builders<DimJobRoleMongoDocument>.IndexKeys.Ascending(x => x.StandardJobRoleId);
        await _jobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimJobRoleMongoDocument>(standardJobRoleIdIndex, new CreateIndexOptions { Name = "idx_job_role_sjr_id" })
        );
        var hierarchyLevelIdIndex = Builders<DimJobRoleMongoDocument>.IndexKeys.Ascending(x => x.HierarchyLevelId);
        await _jobRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimJobRoleMongoDocument>(hierarchyLevelIdIndex, new CreateIndexOptions { Name = "idx_job_role_hl_id" })
        );

        _logger.LogInformation("Ensured indexes for 'dim_job_roles' collection.");
    }

    private DimJobRole ToDomain(DimJobRoleMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimJobRole
        {
            JobRoleId = doc.JobRoleId,
            JobRoleTitle = doc.JobRoleTitle,
            StandardJobRoleId = doc.StandardJobRoleId,
            HierarchyLevelId = doc.HierarchyLevelId
        };
    }

    private DimJobRoleMongoDocument FromDomain(DimJobRole domain)
    {
        if (domain == null) return null!;
        return new DimJobRoleMongoDocument
        {
            JobRoleId = domain.JobRoleId,
            JobRoleTitle = domain.JobRoleTitle,
            StandardJobRoleId = domain.StandardJobRoleId,
            HierarchyLevelId = domain.HierarchyLevelId
        };
    }
    
    public async Task AddJobRoleAsync(DimJobRole jobRole)
    {
        _logger.LogInformation("MongoRepo: Attempting to add job role: {JobRoleTitle}", jobRole.JobRoleTitle);
        if (jobRole.JobRoleId == 0)
        {
            jobRole.JobRoleId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "job_role_id");
            _logger.LogInformation("MongoRepo: Generated new JobRoleId {JobRoleId} for {JobRoleTitle}", 
                                   jobRole.JobRoleId, jobRole.JobRoleTitle);
        }

        var document = FromDomain(jobRole);
        
        try
        {
            await _jobRolesCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Job role '{JobRoleTitle}' added with JobRoleId {JobRoleId}, ObjectId {ObjectId}", 
                                   document.JobRoleTitle, document.JobRoleId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding job role '{JobRoleTitle}'. It might already exist (natural key or JobRoleId).", jobRole.JobRoleTitle);
            string errorMessage = $"A job role with the title '{jobRole.JobRoleTitle}' and associated IDs already exists, or JobRoleId {jobRole.JobRoleId} is a duplicate.";
            if (mwx.Message.Contains("idx_job_role_natural_key_unique")) errorMessage = $"A job role with title '{jobRole.JobRoleTitle}', StandardJobRoleId {jobRole.StandardJobRoleId}, and HierarchyLevelId {jobRole.HierarchyLevelId} already exists.";
            else if (mwx.Message.Contains("idx_job_role_id_unique")) errorMessage = $"JobRoleId {jobRole.JobRoleId} conflict (should not happen with sequence).";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId)
    {
        _logger.LogDebug("MongoRepo: Getting job role by JobRoleId: {JobRoleId}", jobRoleId);
        var filter = Builders<DimJobRoleMongoDocument>.Filter.Eq(doc => doc.JobRoleId, jobRoleId);
        var document = await _jobRolesCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Job role with JobRoleId {JobRoleId} not found.", jobRoleId);
            throw new NotFoundException($"Job role with ID {jobRoleId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all job roles.");
        var documents = await _jobRolesCollection.Find(FilterDefinition<DimJobRoleMongoDocument>.Empty)
                                                 .Sort(Builders<DimJobRoleMongoDocument>.Sort.Ascending(x => x.JobRoleTitle))
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateJobRoleAsync(DimJobRole jobRole)
    {
        _logger.LogInformation("MongoRepo: Attempting to update job role with JobRoleId: {JobRoleId}", jobRole.JobRoleId);
        var filter = Builders<DimJobRoleMongoDocument>.Filter.Eq(doc => doc.JobRoleId, jobRole.JobRoleId);
        
        var existingDocument = await _jobRolesCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            _logger.LogWarning("MongoRepo: JobRoleId {Id} not found for update.", jobRole.JobRoleId);
            throw new NotFoundException($"Job role with ID {jobRole.JobRoleId} not found for update.");
        }
        
        existingDocument.JobRoleTitle = jobRole.JobRoleTitle;
        existingDocument.StandardJobRoleId = jobRole.StandardJobRoleId;
        existingDocument.HierarchyLevelId = jobRole.HierarchyLevelId;

        try
        {
            var result = await _jobRolesCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: JobRoleId {Id} not matched during ReplaceOne operation.", jobRole.JobRoleId);
                throw new NotFoundException($"Job role with ID {jobRole.JobRoleId} not found for update (concurrent modification?).");
            }
            _logger.LogInformation("MongoRepo: Job role with JobRoleId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   jobRole.JobRoleId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating job role JobRoleId {Id} (likely new natural key conflicts).", 
                               jobRole.JobRoleId);
            throw new ConflictException($"Updating job role resulted in a conflict (e.g., title '{jobRole.JobRoleTitle}' with StandardJobRoleId {jobRole.StandardJobRoleId} and HierarchyLevelId {jobRole.HierarchyLevelId} already exists).");
        }
    }

    public async Task DeleteJobRoleAsync(int jobRoleId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete job role with JobRoleId: {JobRoleId}", jobRoleId);
        var filter = Builders<DimJobRoleMongoDocument>.Filter.Eq(doc => doc.JobRoleId, jobRoleId);
        var result = await _jobRolesCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Job role with JobRoleId {JobRoleId} not found for deletion.", jobRoleId);
            throw new NotFoundException($"Job role with ID {jobRoleId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Job role with JobRoleId {JobRoleId} deleted. Count: {DeletedCount}", 
                               jobRoleId, result.DeletedCount);
    }
}