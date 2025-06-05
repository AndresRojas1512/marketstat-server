using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimStandardJobRoleHierarchyRepository : IDimStandardJobRoleHierarchyRepository
{
    private readonly IMongoCollection<DimStandardJobRoleHierarchyMongoDocument> _linksCollection;
    private readonly ILogger<MongoDimStandardJobRoleHierarchyRepository> _logger;

    public MongoDimStandardJobRoleHierarchyRepository(IMongoDatabase database, ILogger<MongoDimStandardJobRoleHierarchyRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _linksCollection = database.GetCollection<DimStandardJobRoleHierarchyMongoDocument>("dim_standard_job_role_hierarchies");
    }

    public async Task CreateIndexesAsync()
    {
        var compositeKeyIndex = Builders<DimStandardJobRoleHierarchyMongoDocument>.IndexKeys
            .Ascending(x => x.StandardJobRoleId)
            .Ascending(x => x.HierarchyLevelId);
        await _linksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimStandardJobRoleHierarchyMongoDocument>(compositeKeyIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_stdjobrole_hierarchy_unique" })
        );

        var jobRoleIdIndex = Builders<DimStandardJobRoleHierarchyMongoDocument>.IndexKeys.Ascending(x => x.StandardJobRoleId);
        await _linksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimStandardJobRoleHierarchyMongoDocument>(jobRoleIdIndex, 
            new CreateIndexOptions { Name = "idx_link_std_job_role_id" })
        );
        
        var levelIdIndex = Builders<DimStandardJobRoleHierarchyMongoDocument>.IndexKeys.Ascending(x => x.HierarchyLevelId);
         await _linksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimStandardJobRoleHierarchyMongoDocument>(levelIdIndex, 
            new CreateIndexOptions { Name = "idx_link_hierarchy_level_id" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_standard_job_role_hierarchies' collection.");
    }

    private DimStandardJobRoleHierarchy ToDomain(DimStandardJobRoleHierarchyMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimStandardJobRoleHierarchy
        {
            StandardJobRoleId = doc.StandardJobRoleId,
            HierarchyLevelId = doc.HierarchyLevelId
        };
    }

    private DimStandardJobRoleHierarchyMongoDocument FromDomain(DimStandardJobRoleHierarchy domain)
    {
        if (domain == null) return null!;
        return new DimStandardJobRoleHierarchyMongoDocument
        {
            StandardJobRoleId = domain.StandardJobRoleId,
            HierarchyLevelId = domain.HierarchyLevelId
        };
    }
    
    public async Task AddStandardJobRoleHierarchyAsync(DimStandardJobRoleHierarchy link)
    {
        _logger.LogInformation("MongoRepo: Attempting to add StandardJobRole-Hierarchy link: SJR_ID {SJR_ID}, HL_ID {HL_ID}", 
                               link.StandardJobRoleId, link.HierarchyLevelId);
        var document = FromDomain(link);
        
        try
        {
            await _linksCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Link added for SJR_ID {SJR_ID}, HL_ID {HL_ID} with ObjectId {ObjectId}", 
                                   document.StandardJobRoleId, document.HierarchyLevelId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding link for SJR_ID {SJR_ID}, HL_ID {HL_ID}.", 
                               link.StandardJobRoleId, link.HierarchyLevelId);
            throw new ConflictException($"The link between StandardJobRole {link.StandardJobRoleId} & HierarchyLevel {link.HierarchyLevelId} already exists.");
        }
    }

    public async Task<DimStandardJobRoleHierarchy> GetStandardJobRoleHierarchyAsync(int jobRoleId, int levelId)
    {
        _logger.LogDebug("MongoRepo: Getting link for SJR_ID: {SJR_ID}, HL_ID: {HL_ID}", 
                         jobRoleId, levelId);
        var filter = Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.And(
            Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.Eq(doc => doc.StandardJobRoleId, jobRoleId),
            Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.Eq(doc => doc.HierarchyLevelId, levelId)
        );
        var document = await _linksCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Link for SJR_ID {SJR_ID}, HL_ID {HL_ID} not found.", 
                               jobRoleId, levelId);
            throw new NotFoundException($"Link for StandardJobRole {jobRoleId} & HierarchyLevel {levelId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetLevelsByJobRoleIdAsync(int jobRoleId)
    {
        _logger.LogDebug("MongoRepo: Getting hierarchy levels for SJR_ID: {SJR_ID}", jobRoleId);
        var filter = Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.Eq(doc => doc.StandardJobRoleId, jobRoleId);
        var documents = await _linksCollection.Find(filter)
                                             .Sort(Builders<DimStandardJobRoleHierarchyMongoDocument>.Sort.Ascending(x => x.HierarchyLevelId))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetJobRolesByLevelIdAsync(int levelId)
    {
        _logger.LogDebug("MongoRepo: Getting standard job roles for HL_ID: {HL_ID}", levelId);
        var filter = Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.Eq(doc => doc.HierarchyLevelId, levelId);
        var documents = await _linksCollection.Find(filter)
                                             .Sort(Builders<DimStandardJobRoleHierarchyMongoDocument>.Sort.Ascending(x => x.StandardJobRoleId))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetAllStandardJobRoleHierarchiesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all standard job role-hierarchy links.");
        var documents = await _linksCollection.Find(FilterDefinition<DimStandardJobRoleHierarchyMongoDocument>.Empty)
                                             .Sort(Builders<DimStandardJobRoleHierarchyMongoDocument>.Sort.Ascending(x => x.StandardJobRoleId).Ascending(x => x.HierarchyLevelId))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task DeleteStandardJobRoleHierarchyAsync(int jobRoleId, int levelId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete link for SJR_ID: {SJR_ID}, HL_ID: {HL_ID}", 
                               jobRoleId, levelId);
        var filter = Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.And(
            Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.Eq(doc => doc.StandardJobRoleId, jobRoleId),
            Builders<DimStandardJobRoleHierarchyMongoDocument>.Filter.Eq(doc => doc.HierarchyLevelId, levelId)
        );
        var result = await _linksCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
             _logger.LogWarning("MongoRepo: Link for SJR_ID {SJR_ID}, HL_ID {HL_ID} not found for deletion.", 
                               jobRoleId, levelId);
            throw new NotFoundException($"Link for StandardJobRole {jobRoleId} & HierarchyLevel {levelId} not found for deletion.");
        }
         _logger.LogInformation("MongoRepo: Link for SJR_ID {SJR_ID}, HL_ID {HL_ID} deleted. Count: {DeletedCount}", 
                               jobRoleId, levelId, result.DeletedCount);
    }
}