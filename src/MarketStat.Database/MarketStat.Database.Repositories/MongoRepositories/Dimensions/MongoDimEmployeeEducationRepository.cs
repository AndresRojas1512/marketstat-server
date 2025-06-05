using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimEmployeeEducationRepository : IDimEmployeeEducationRepository
{
    private readonly IMongoCollection<DimEmployeeEducationMongoDocument> _employeeEducationsCollection;
    private readonly ILogger<MongoDimEmployeeEducationRepository> _logger;

    public MongoDimEmployeeEducationRepository(IMongoDatabase database, ILogger<MongoDimEmployeeEducationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _employeeEducationsCollection = database.GetCollection<DimEmployeeEducationMongoDocument>("dim_employee_educations");
    }

    public async Task CreateIndexesAsync()
    {
        var compositeKeyIndex = Builders<DimEmployeeEducationMongoDocument>.IndexKeys
            .Ascending(x => x.EmployeeId)
            .Ascending(x => x.EducationId);
        await _employeeEducationsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployeeEducationMongoDocument>(compositeKeyIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_employee_education_unique" })
        );

        // Index on employee_id for faster GetEducationsByEmployeeIdAsync
        var employeeIdIndex = Builders<DimEmployeeEducationMongoDocument>.IndexKeys.Ascending(x => x.EmployeeId);
        await _employeeEducationsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployeeEducationMongoDocument>(employeeIdIndex, 
            new CreateIndexOptions { Name = "idx_employee_id_for_educations" })
        );
        
        // Index on education_id if you ever need to query by it
        var educationIdIndex = Builders<DimEmployeeEducationMongoDocument>.IndexKeys.Ascending(x => x.EducationId);
         await _employeeEducationsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployeeEducationMongoDocument>(educationIdIndex, 
            new CreateIndexOptions { Name = "idx_education_id_for_employees" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_employee_educations' collection.");
    }

    private DimEmployeeEducation ToDomain(DimEmployeeEducationMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimEmployeeEducation
        {
            EmployeeId = doc.EmployeeId,
            EducationId = doc.EducationId,
            GraduationYear = doc.GraduationYear
        };
    }

    private DimEmployeeEducationMongoDocument FromDomain(DimEmployeeEducation domain)
    {
        if (domain == null) return null!;
        return new DimEmployeeEducationMongoDocument
        {
            EmployeeId = domain.EmployeeId,
            EducationId = domain.EducationId,
            GraduationYear = domain.GraduationYear
        };
    }

    public async Task AddEmployeeEducationAsync(DimEmployeeEducation link)
    {
        _logger.LogInformation("MongoRepo: Attempting to add employee-education link: EmployeeId {EmployeeId}, EducationId {EducationId}", 
                               link.EmployeeId, link.EducationId);
        var document = FromDomain(link);
        
        try
        {
            await _employeeEducationsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Employee-education link added for EmployeeId {EmployeeId}, EducationId {EducationId} with ObjectId {ObjectId}", 
                                   document.EmployeeId, document.EducationId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding employee-education link for EmployeeId {EmployeeId}, EducationId {EducationId}.", 
                               link.EmployeeId, link.EducationId);
            throw new ConflictException($"Employee {link.EmployeeId} is already linked with education {link.EducationId}.");
        }
        // Note: FK validation for EmployeeId and EducationId would need to occur in the service layer by checking
        // the respective collections if absolute referential integrity is required before insert.
    }

    public async Task<DimEmployeeEducation> GetEmployeeEducationAsync(int employeeId, int educationId)
    {
        _logger.LogDebug("MongoRepo: Getting employee-education link for EmployeeId: {EmployeeId}, EducationId: {EducationId}", 
                         employeeId, educationId);
        var filter = Builders<DimEmployeeEducationMongoDocument>.Filter.And(
            Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EmployeeId, employeeId),
            Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EducationId, educationId)
        );
        var document = await _employeeEducationsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Employee-education link for EmployeeId {EmployeeId}, EducationId {EducationId} not found.", 
                               employeeId, educationId);
            throw new NotFoundException($"Link ({employeeId}, {educationId}) not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId)
    {
        _logger.LogDebug("MongoRepo: Getting educations for EmployeeId: {EmployeeId}", employeeId);
        var filter = Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EmployeeId, employeeId);
        var documents = await _employeeEducationsCollection.Find(filter)
                                                         .Sort(Builders<DimEmployeeEducationMongoDocument>.Sort.Ascending(x => x.EducationId)) // Optional sort
                                                         .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimEmployeeEducation>> GetAllEmployeeEducationsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all employee-education links.");
        var documents = await _employeeEducationsCollection.Find(FilterDefinition<DimEmployeeEducationMongoDocument>.Empty)
                                                         .Sort(Builders<DimEmployeeEducationMongoDocument>.Sort.Ascending(x => x.EmployeeId).Ascending(x => x.EducationId)) // Optional sort
                                                         .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateEmployeeEducationAsync(DimEmployeeEducation link)
    {
        _logger.LogInformation("MongoRepo: Attempting to update employee-education link for EmployeeId: {EmployeeId}, EducationId: {EducationId}", 
                               link.EmployeeId, link.EducationId);
        var filter = Builders<DimEmployeeEducationMongoDocument>.Filter.And(
            Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EmployeeId, link.EmployeeId),
            Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EducationId, link.EducationId)
        );
        
        // Only GraduationYear is typically updatable for this link table.
        var updateDefinition = Builders<DimEmployeeEducationMongoDocument>.Update
            .Set(doc => doc.GraduationYear, link.GraduationYear);

        var result = await _employeeEducationsCollection.UpdateOneAsync(filter, updateDefinition);

        if (result.MatchedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Employee-education link for EmployeeId {EmployeeId}, EducationId {EducationId} not found for update.", 
                               link.EmployeeId, link.EducationId);
            throw new NotFoundException($"Link ({link.EmployeeId}, {link.EducationId}) not found for update.");
        }
        _logger.LogInformation("MongoRepo: Employee-education link for EmployeeId {EmployeeId}, EducationId {EducationId} updated. Matched: {Matched}, Modified: {Modified}", 
                               link.EmployeeId, link.EducationId, result.MatchedCount, result.ModifiedCount);
    }
    
    public async Task DeleteEmployeeEducationAsync(int employeeId, int educationId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete employee-education link for EmployeeId: {EmployeeId}, EducationId: {EducationId}", 
                               employeeId, educationId);
        var filter = Builders<DimEmployeeEducationMongoDocument>.Filter.And(
            Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EmployeeId, employeeId),
            Builders<DimEmployeeEducationMongoDocument>.Filter.Eq(doc => doc.EducationId, educationId)
        );
        var result = await _employeeEducationsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Employee-education link for EmployeeId {EmployeeId}, EducationId {EducationId} not found for deletion.", 
                               employeeId, educationId);
            throw new NotFoundException($"Link ({employeeId}, {educationId}) not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Employee-education link for EmployeeId {EmployeeId}, EducationId {EducationId} deleted. Count: {DeletedCount}", 
                               employeeId, educationId, result.DeletedCount);
    }
}