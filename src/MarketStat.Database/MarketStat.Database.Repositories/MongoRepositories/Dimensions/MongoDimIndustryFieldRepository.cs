using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimIndustryFieldRepository : IDimIndustryFieldRepository
{
    private readonly IMongoCollection<DimIndustryFieldMongoDocument> _industryFieldsCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimIndustryFieldRepository> _logger;

    public MongoDimIndustryFieldRepository(IMongoDatabase database, ILogger<MongoDimIndustryFieldRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _industryFieldsCollection = database.GetCollection<DimIndustryFieldMongoDocument>("dim_industry_fields");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var industryFieldIdIndex = Builders<DimIndustryFieldMongoDocument>.IndexKeys.Ascending(x => x.IndustryFieldId);
        await _industryFieldsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimIndustryFieldMongoDocument>(industryFieldIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_industry_field_id_unique" })
        );

        // Unique index on IndustryFieldName
        var nameIndex = Builders<DimIndustryFieldMongoDocument>.IndexKeys.Ascending(x => x.IndustryFieldName);
        await _industryFieldsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimIndustryFieldMongoDocument>(nameIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_industry_field_name_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_industry_fields' collection.");
    }

    private DimIndustryField ToDomain(DimIndustryFieldMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimIndustryField
        {
            IndustryFieldId = doc.IndustryFieldId,
            IndustryFieldName = doc.IndustryFieldName
        };
    }

    private DimIndustryFieldMongoDocument FromDomain(DimIndustryField domain)
    {
        if (domain == null) return null!;
        return new DimIndustryFieldMongoDocument
        {
            IndustryFieldId = domain.IndustryFieldId,
            IndustryFieldName = domain.IndustryFieldName
        };
    }
    
    public async Task AddIndustryFieldAsync(DimIndustryField industryField)
    {
        _logger.LogInformation("MongoRepo: Attempting to add industry field: {IndustryFieldName}", industryField.IndustryFieldName);
        if (industryField.IndustryFieldId == 0)
        {
            industryField.IndustryFieldId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "industry_field_id");
            _logger.LogInformation("MongoRepo: Generated new IndustryFieldId {IndustryFieldId} for {IndustryFieldName}", 
                                   industryField.IndustryFieldId, industryField.IndustryFieldName);
        }

        var document = FromDomain(industryField);
        
        try
        {
            await _industryFieldsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Industry field '{IndustryFieldName}' added with IndustryFieldId {IndustryFieldId}, ObjectId {ObjectId}", 
                                   document.IndustryFieldName, document.IndustryFieldId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding industry field '{IndustryFieldName}'. It might already exist (name or IndustryFieldId).", industryField.IndustryFieldName);
            string errorMessage = $"An industry field with the name '{industryField.IndustryFieldName}' or ID {industryField.IndustryFieldId} already exists.";
             if(mwx.Message.Contains("idx_industry_field_name_unique")) errorMessage = $"An industry field named '{industryField.IndustryFieldName}' already exists.";
             else if (mwx.Message.Contains("idx_industry_field_id_unique")) errorMessage = $"IndustryFieldId {industryField.IndustryFieldId} conflict.";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        _logger.LogDebug("MongoRepo: Getting industry field by IndustryFieldId: {IndustryFieldId}", industryFieldId);
        var filter = Builders<DimIndustryFieldMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryFieldId);
        var document = await _industryFieldsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Industry field with IndustryFieldId {IndustryFieldId} not found.", industryFieldId);
            throw new NotFoundException($"Industry field with ID {industryFieldId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all industry fields.");
        var documents = await _industryFieldsCollection.Find(FilterDefinition<DimIndustryFieldMongoDocument>.Empty)
                                                       .Sort(Builders<DimIndustryFieldMongoDocument>.Sort.Ascending(x => x.IndustryFieldName))
                                                       .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateIndustryFieldAsync(DimIndustryField industryField)
    {
        _logger.LogInformation("MongoRepo: Attempting to update industry field with IndustryFieldId: {IndustryFieldId}", industryField.IndustryFieldId);
        var filter = Builders<DimIndustryFieldMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryField.IndustryFieldId);
        
        var existingDocument = await _industryFieldsCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            _logger.LogWarning("MongoRepo: IndustryFieldId {Id} not found for update.", industryField.IndustryFieldId);
            throw new NotFoundException($"Industry field with ID {industryField.IndustryFieldId} not found for update.");
        }
        
        existingDocument.IndustryFieldName = industryField.IndustryFieldName;

        try
        {
            var result = await _industryFieldsCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: IndustryFieldId {Id} not matched during ReplaceOne operation.", industryField.IndustryFieldId);
                throw new NotFoundException($"Industry field with ID {industryField.IndustryFieldId} not found for update (concurrent modification?).");
            }
            _logger.LogInformation("MongoRepo: Industry field with IndustryFieldId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   industryField.IndustryFieldId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating industry field IndustryFieldId {Id} to name '{Name}'.", 
                               industryField.IndustryFieldId, industryField.IndustryFieldName);
            throw new ConflictException($"An industry field named '{industryField.IndustryFieldName}' already exists.");
        }
    }

    public async Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete industry field with IndustryFieldId: {IndustryFieldId}", industryFieldId);
        var filter = Builders<DimIndustryFieldMongoDocument>.Filter.Eq(doc => doc.IndustryFieldId, industryFieldId);
        var result = await _industryFieldsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Industry field with IndustryFieldId {IndustryFieldId} not found for deletion.", industryFieldId);
            throw new NotFoundException($"Industry field with ID {industryFieldId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Industry field with IndustryFieldId {IndustryFieldId} deleted. Count: {DeletedCount}", 
                               industryFieldId, result.DeletedCount);
    }
}