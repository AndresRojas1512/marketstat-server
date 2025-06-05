using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimEmployerIndustryFieldRepository : IDimEmployerIndustryFieldRepository
{
    private readonly IMongoCollection<DimEmployerIndustryFieldMongoDocument> _linksCollection;
    private readonly ILogger<MongoDimEmployerIndustryFieldRepository> _logger;

    public MongoDimEmployerIndustryFieldRepository(IMongoDatabase database, ILogger<MongoDimEmployerIndustryFieldRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _linksCollection = database.GetCollection<DimEmployerIndustryFieldMongoDocument>("dim_employer_industry_fields");
    }

    public async Task CreateIndexesAsync()
    {
        var compositeKeyIndex = Builders<DimEmployerIndustryFieldMongoDocument>.IndexKeys
            .Ascending(x => x.EmployerId)
            .Ascending(x => x.IndustryFieldId);
        await _linksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerIndustryFieldMongoDocument>(compositeKeyIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_employer_industry_field_unique" })
        );

        var employerIdIndex = Builders<DimEmployerIndustryFieldMongoDocument>.IndexKeys.Ascending(x => x.EmployerId);
        await _linksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerIndustryFieldMongoDocument>(employerIdIndex, 
            new CreateIndexOptions { Name = "idx_link_employer_id" })
        );
        
        var industryFieldIdIndex = Builders<DimEmployerIndustryFieldMongoDocument>.IndexKeys.Ascending(x => x.IndustryFieldId);
         await _linksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerIndustryFieldMongoDocument>(industryFieldIdIndex, 
            new CreateIndexOptions { Name = "idx_link_industry_field_id" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_employer_industry_fields' collection.");
    }

    private DimEmployerIndustryField ToDomain(DimEmployerIndustryFieldMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimEmployerIndustryField
        {
            EmployerId = doc.EmployerId,
            IndustryFieldId = doc.IndustryFieldId
        };
    }

    private DimEmployerIndustryFieldMongoDocument FromDomain(DimEmployerIndustryField domain)
    {
        if (domain == null) return null!;
        return new DimEmployerIndustryFieldMongoDocument
        {
            EmployerId = domain.EmployerId,
            IndustryFieldId = domain.IndustryFieldId
        };
    }
    
    public async Task AddEmployerIndustryFieldAsync(DimEmployerIndustryField link)
    {
        _logger.LogInformation("MongoRepo: Attempting to add employer-industry link: EmployerId {EmployerId}, IndustryFieldId {IndustryFieldId}", 
                               link.EmployerId, link.IndustryFieldId);
        var document = FromDomain(link);
        
        try
        {
            await _linksCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Employer-industry link added for EmpId {EmpId}, IndFieldId {IndFieldId} with ObjectId {ObjectId}", 
                                   document.EmployerId, document.IndustryFieldId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding link for EmployerId {EmployerId}, IndustryFieldId {IndustryFieldId}.", 
                               link.EmployerId, link.IndustryFieldId);
            throw new ConflictException($"The link between employer {link.EmployerId} & industry field {link.IndustryFieldId} already exists.");
        }
    }

    public async Task<DimEmployerIndustryField> GetEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        _logger.LogDebug("MongoRepo: Getting link for EmployerId: {EmployerId}, IndustryFieldId: {IndustryFieldId}", 
                         employerId, industryFieldId);
        var filter = Builders<DimEmployerIndustryFieldMongoDocument>.Filter.And(
            Builders<DimEmployerIndustryFieldMongoDocument>.Filter.Eq(doc => doc.EmployerId, employerId),
            Builders<DimEmployerIndustryFieldMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryFieldId)
        );
        var document = await _linksCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Link for EmployerId {EmployerId}, IndustryFieldId {IndustryFieldId} not found.", 
                               employerId, industryFieldId);
            throw new NotFoundException($"Link for employer {employerId} & industry field {industryFieldId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetIndustryFieldsByEmployerIdAsync(int employerId)
    {
        _logger.LogDebug("MongoRepo: Getting industry fields for EmployerId: {EmployerId}", employerId);
        var filter = Builders<DimEmployerIndustryFieldMongoDocument>.Filter.Eq(doc => doc.EmployerId, employerId);
        var documents = await _linksCollection.Find(filter)
                                             .Sort(Builders<DimEmployerIndustryFieldMongoDocument>.Sort.Ascending(x => x.IndustryFieldId))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetEmployersByIndustryFieldIdAsync(int industryFieldId)
    {
        _logger.LogDebug("MongoRepo: Getting employers for IndustryFieldId: {IndustryFieldId}", industryFieldId);
        var filter = Builders<DimEmployerIndustryFieldMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryFieldId);
        var documents = await _linksCollection.Find(filter)
                                             .Sort(Builders<DimEmployerIndustryFieldMongoDocument>.Sort.Ascending(x => x.EmployerId))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetAllEmployerIndustryFieldsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all employer-industry field links.");
        var documents = await _linksCollection.Find(FilterDefinition<DimEmployerIndustryFieldMongoDocument>.Empty)
                                             .Sort(Builders<DimEmployerIndustryFieldMongoDocument>.Sort.Ascending(x => x.EmployerId).Ascending(x => x.IndustryFieldId))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task DeleteEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete link for EmployerId: {EmployerId}, IndustryFieldId: {IndustryFieldId}", 
                               employerId, industryFieldId);
        var filter = Builders<DimEmployerIndustryFieldMongoDocument>.Filter.And(
            Builders<DimEmployerIndustryFieldMongoDocument>.Filter.Eq(doc => doc.EmployerId, employerId),
            Builders<DimEmployerIndustryFieldMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryFieldId)
        );
        var result = await _linksCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
             _logger.LogWarning("MongoRepo: Link for EmployerId {EmployerId}, IndustryFieldId {IndustryFieldId} not found for deletion.", 
                               employerId, industryFieldId);
            throw new NotFoundException($"Link for employer {employerId} & industry field {industryFieldId} not found for deletion.");
        }
         _logger.LogInformation("MongoRepo: Link for EmployerId {EmployerId}, IndustryFieldId {IndustryFieldId} deleted. Count: {DeletedCount}", 
                               employerId, industryFieldId, result.DeletedCount);
    }
}