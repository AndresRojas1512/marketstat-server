using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimEducationRepository : IDimEducationRepository
{
    private readonly IMongoCollection<DimEducationMongoDocument> _educationsCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimEducationRepository> _logger;

    public MongoDimEducationRepository(IMongoDatabase database, ILogger<MongoDimEducationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _educationsCollection = database.GetCollection<DimEducationMongoDocument>("dim_educations");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var educationIdIndex = Builders<DimEducationMongoDocument>.IndexKeys.Ascending(x => x.EducationId);
        await _educationsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEducationMongoDocument>(educationIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_education_id_unique" })
        );

        var specialtyCodeIndex = Builders<DimEducationMongoDocument>.IndexKeys.Ascending(x => x.SpecialtyCode);
        await _educationsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEducationMongoDocument>(specialtyCodeIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_specialty_code_unique" })
        );
        
        // Index on education_level_id if you query by it frequently
        var educationLevelIdIndex = Builders<DimEducationMongoDocument>.IndexKeys.Ascending(x => x.EducationLevelId);
        await _educationsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEducationMongoDocument>(educationLevelIdIndex, 
            new CreateIndexOptions { Name = "idx_education_level_id" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_educations' collection.");
    }

    private DimEducation ToDomain(DimEducationMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimEducation
        {
            EducationId = doc.EducationId,
            Specialty = doc.Specialty,
            SpecialtyCode = doc.SpecialtyCode,
            EducationLevelId = doc.EducationLevelId
        };
    }

    private DimEducationMongoDocument FromDomain(DimEducation domain)
    {
        if (domain == null) return null!;
        return new DimEducationMongoDocument
        {
            EducationId = domain.EducationId,
            Specialty = domain.Specialty,
            SpecialtyCode = domain.SpecialtyCode,
            EducationLevelId = domain.EducationLevelId
        };
    }

    public async Task AddEducationAsync(DimEducation education)
    {
        _logger.LogInformation("MongoRepo: Attempting to add education: Specialty '{Specialty}', Code '{SpecialtyCode}'", 
                               education.Specialty, education.SpecialtyCode);
        if (education.EducationId == 0)
        {
            education.EducationId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "education_id");
            _logger.LogInformation("MongoRepo: Generated new EducationId {EducationId} for Specialty '{Specialty}'", 
                                   education.EducationId, education.Specialty);
        }

        var document = FromDomain(education);
        
        try
        {
            await _educationsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Education '{Specialty}' (Code: {SpecialtyCode}) added with EducationId {EducationId}, ObjectId {ObjectId}", 
                                   document.Specialty, document.SpecialtyCode, document.EducationId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding education with SpecialtyCode '{SpecialtyCode}' or EducationId {EducationId}.", 
                               education.SpecialtyCode, education.EducationId);
            string errorMessage = $"An education record with SpecialtyCode '{education.SpecialtyCode}' or ID {education.EducationId} already exists.";
             if(mwx.Message.Contains("idx_specialty_code_unique")) errorMessage = $"An education with SpecialtyCode '{education.SpecialtyCode}' already exists.";
             else if (mwx.Message.Contains("idx_education_id_unique")) errorMessage = $"EducationId {education.EducationId} conflict.";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimEducation> GetEducationByIdAsync(int educationId)
    {
        _logger.LogDebug("MongoRepo: Getting education by EducationId: {EducationId}", educationId);
        var filter = Builders<DimEducationMongoDocument>.Filter.Eq(doc => doc.EducationId, educationId);
        var document = await _educationsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Education with EducationId {EducationId} not found.", educationId);
            throw new NotFoundException($"Education with ID {educationId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimEducation>> GetAllEducationsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all educations.");
        var documents = await _educationsCollection.Find(FilterDefinition<DimEducationMongoDocument>.Empty)
                                                 .Sort(Builders<DimEducationMongoDocument>.Sort.Ascending(x => x.Specialty))
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateEducationAsync(DimEducation education)
    {
        _logger.LogInformation("MongoRepo: Attempting to update education with EducationId: {EducationId}", education.EducationId);
        var filter = Builders<DimEducationMongoDocument>.Filter.Eq(doc => doc.EducationId, education.EducationId);
        
        var existingDocument = await _educationsCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            _logger.LogWarning("MongoRepo: EducationId {Id} not found for update.", education.EducationId);
            throw new NotFoundException($"Education with ID {education.EducationId} not found for update.");
        }
        
        existingDocument.Specialty = education.Specialty;
        existingDocument.SpecialtyCode = education.SpecialtyCode;
        existingDocument.EducationLevelId = education.EducationLevelId;

        try
        {
            var result = await _educationsCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                 _logger.LogWarning("MongoRepo: EducationId {Id} not matched during ReplaceOne operation.", education.EducationId);
                throw new NotFoundException($"Education with ID {education.EducationId} not found for update (concurrent modification?).");
            }
             _logger.LogInformation("MongoRepo: Education with EducationId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   education.EducationId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating education EducationId {Id} (likely new SpecialtyCode conflicts).", 
                               education.EducationId);
            throw new ConflictException($"Updating education record resulted in a conflict (e.g., SpecialtyCode '{education.SpecialtyCode}' already exists).");
        }
    }

    public async Task DeleteEducationAsync(int educationId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete education with EducationId: {EducationId}", educationId);
        var filter = Builders<DimEducationMongoDocument>.Filter.Eq(doc => doc.EducationId, educationId);
        var result = await _educationsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
             _logger.LogWarning("MongoRepo: Education with EducationId {EducationId} not found for deletion.", educationId);
            throw new NotFoundException($"Education with ID {educationId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Education with EducationId {EducationId} deleted. Count: {DeletedCount}", 
                               educationId, result.DeletedCount);
    }
}