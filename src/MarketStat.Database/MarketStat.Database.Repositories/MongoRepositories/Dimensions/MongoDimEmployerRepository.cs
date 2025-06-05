using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimEmployerRepository : IDimEmployerRepository
{
    private readonly IMongoCollection<DimEmployerMongoDocument> _employersCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimEmployerRepository> _logger;

    public MongoDimEmployerRepository(IMongoDatabase database, ILogger<MongoDimEmployerRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _employersCollection = database.GetCollection<DimEmployerMongoDocument>("dim_employers");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var employerIdIndex = Builders<DimEmployerMongoDocument>.IndexKeys.Ascending(x => x.EmployerId);
        await _employersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerMongoDocument>(employerIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_employer_id_unique" })
        );

        var nameIndex = Builders<DimEmployerMongoDocument>.IndexKeys.Ascending(x => x.EmployerName);
        await _employersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerMongoDocument>(nameIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_employer_name_unique" })
        );

        var innIndex = Builders<DimEmployerMongoDocument>.IndexKeys.Ascending(x => x.Inn);
        await _employersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerMongoDocument>(innIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_inn_unique" })
        );
        
        var ogrnIndex = Builders<DimEmployerMongoDocument>.IndexKeys.Ascending(x => x.Ogrn);
        await _employersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployerMongoDocument>(ogrnIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_ogrn_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_employers' collection.");
    }

    private DimEmployer ToDomain(DimEmployerMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimEmployer
        {
            EmployerId = doc.EmployerId,
            EmployerName = doc.EmployerName,
            Inn = doc.Inn,
            Ogrn = doc.Ogrn,
            Kpp = doc.Kpp,
            RegistrationDate = DateOnly.FromDateTime(doc.RegistrationDate),
            LegalAddress = doc.LegalAddress,
            Website = doc.Website,
            ContactEmail = doc.ContactEmail,
            ContactPhone = doc.ContactPhone
        };
    }

    private DimEmployerMongoDocument FromDomain(DimEmployer domain)
    {
        if (domain == null) return null!;
        return new DimEmployerMongoDocument
        {
            EmployerId = domain.EmployerId,
            EmployerName = domain.EmployerName,
            Inn = domain.Inn,
            Ogrn = domain.Ogrn,
            Kpp = domain.Kpp,
            RegistrationDate = domain.RegistrationDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            LegalAddress = domain.LegalAddress,
            Website = domain.Website,
            ContactEmail = domain.ContactEmail,
            ContactPhone = domain.ContactPhone
            // MongoDB ObjectId 'Id' will be generated on insert if not set for the document.
        };
    }

    public async Task AddEmployerAsync(DimEmployer employer)
    {
        _logger.LogInformation("MongoRepo: Attempting to add employer: {EmployerName}", employer.EmployerName);
        if (employer.EmployerId == 0) // New employer, generate ID
        {
            employer.EmployerId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "employer_id");
            _logger.LogInformation("MongoRepo: Generated new EmployerId {EmployerId} for {EmployerName}", 
                                   employer.EmployerId, employer.EmployerName);
        }

        var document = FromDomain(employer);
        
        try
        {
            await _employersCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Employer '{EmployerName}' added with EmployerId {EmployerId}, ObjectId {ObjectId}", 
                                   document.EmployerName, document.EmployerId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding employer '{EmployerName}'. It might already exist (name, INN, OGRN, or EmployerId).", employer.EmployerName);
            // More specific error message based on which index failed
            string conflictField = "unknown unique field";
            if (mwx.Message.Contains("idx_employer_name_unique")) conflictField = $"name '{employer.EmployerName}'";
            else if (mwx.Message.Contains("idx_inn_unique")) conflictField = $"INN '{employer.Inn}'";
            else if (mwx.Message.Contains("idx_ogrn_unique")) conflictField = $"OGRN '{employer.Ogrn}'";
            else if (mwx.Message.Contains("idx_employer_id_unique")) conflictField = $"EmployerId {employer.EmployerId}";
            
            throw new ConflictException($"An employer with the same {conflictField} already exists.");
        }
    }

    public async Task<DimEmployer> GetEmployerByIdAsync(int employerId)
    {
        _logger.LogDebug("MongoRepo: Getting employer by EmployerId: {EmployerId}", employerId);
        var filter = Builders<DimEmployerMongoDocument>.Filter.Eq(doc => doc.EmployerId, employerId);
        var document = await _employersCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Employer with EmployerId {EmployerId} not found.", employerId);
            throw new NotFoundException($"Employer with ID {employerId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all employers.");
        var documents = await _employersCollection.Find(FilterDefinition<DimEmployerMongoDocument>.Empty)
                                                 .Sort(Builders<DimEmployerMongoDocument>.Sort.Ascending(x => x.EmployerName))
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateEmployerAsync(DimEmployer employer)
    {
        _logger.LogInformation("MongoRepo: Attempting to update employer with EmployerId: {EmployerId}", employer.EmployerId);
        var filter = Builders<DimEmployerMongoDocument>.Filter.Eq(doc => doc.EmployerId, employer.EmployerId);
        
        var existingDocument = await _employersCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
             _logger.LogWarning("MongoRepo: EmployerId {Id} not found for update.", employer.EmployerId);
            throw new NotFoundException($"Employer with ID {employer.EmployerId} not found for update.");
        }
        
        existingDocument.EmployerName = employer.EmployerName;
        existingDocument.Inn = employer.Inn;
        existingDocument.Ogrn = employer.Ogrn;
        existingDocument.Kpp = employer.Kpp;
        existingDocument.RegistrationDate = employer.RegistrationDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        existingDocument.LegalAddress = employer.LegalAddress;
        existingDocument.Website = employer.Website;
        existingDocument.ContactEmail = employer.ContactEmail;
        existingDocument.ContactPhone = employer.ContactPhone;

        try
        {
            var result = await _employersCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: EmployerId {Id} not matched during ReplaceOne operation.", employer.EmployerId);
                throw new NotFoundException($"Employer with ID {employer.EmployerId} not found for update (concurrent modification?).");
            }
             _logger.LogInformation("MongoRepo: Employer with EmployerId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   employer.EmployerId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating employer EmployerId {Id}.", employer.EmployerId);
            // Determine which field caused the conflict if possible from mwx.WriteError.Message
            throw new ConflictException("Updating employer record resulted in a conflict (e.g., name, INN, or OGRN already exists for another record).");
        }
    }

    public async Task DeleteEmployerAsync(int employerId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete employer with EmployerId: {EmployerId}", employerId);
        var filter = Builders<DimEmployerMongoDocument>.Filter.Eq(doc => doc.EmployerId, employerId);
        var result = await _employersCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Employer with EmployerId {EmployerId} not found for deletion.", employerId);
            throw new NotFoundException($"Employer with ID {employerId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Employer with EmployerId {EmployeeId} deleted. Count: {DeletedCount}", 
                               employerId, result.DeletedCount);
    }
}