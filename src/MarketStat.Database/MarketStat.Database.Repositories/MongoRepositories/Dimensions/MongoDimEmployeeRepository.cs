using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimEmployeeRepository : IDimEmployeeRepository
{
    private readonly IMongoCollection<DimEmployeeMongoDocument> _employeesCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimEmployeeRepository> _logger;

    public MongoDimEmployeeRepository(IMongoDatabase database, ILogger<MongoDimEmployeeRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _employeesCollection = database.GetCollection<DimEmployeeMongoDocument>("dim_employees");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var employeeIdIndex = Builders<DimEmployeeMongoDocument>.IndexKeys.Ascending(x => x.EmployeeId);
        await _employeesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployeeMongoDocument>(employeeIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_employee_id_unique" })
        );
        
        var datesIndex = Builders<DimEmployeeMongoDocument>.IndexKeys
            .Ascending(x => x.BirthDate)
            .Ascending(x => x.CareerStartDate);
        await _employeesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimEmployeeMongoDocument>(datesIndex, 
            new CreateIndexOptions { Name = "idx_birth_career_start_dates" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_employees' collection.");
    }

    private DimEmployee ToDomain(DimEmployeeMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimEmployee
        {
            EmployeeId = doc.EmployeeId,
            BirthDate = DateOnly.FromDateTime(doc.BirthDate),
            CareerStartDate = DateOnly.FromDateTime(doc.CareerStartDate)
        };
    }

    private DimEmployeeMongoDocument FromDomain(DimEmployee domain)
    {
        if (domain == null) return null!;
        return new DimEmployeeMongoDocument
        {
            EmployeeId = domain.EmployeeId,
            BirthDate = domain.BirthDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            CareerStartDate = domain.CareerStartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
        };
    }

    public async Task AddEmployeeAsync(DimEmployee employee)
    {
        _logger.LogInformation("MongoRepo: Attempting to add employee. BirthDate: {BirthDate}, CareerStart: {CareerStartDate}", 
                               employee.BirthDate, employee.CareerStartDate);
        if (employee.EmployeeId == 0)
        {
            employee.EmployeeId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "employee_id");
            _logger.LogInformation("MongoRepo: Generated new EmployeeId {EmployeeId}", employee.EmployeeId);
        }

        var document = FromDomain(employee);
        
        try
        {
            await _employeesCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Employee added with EmployeeId {EmployeeId}, ObjectId {ObjectId}", 
                                   document.EmployeeId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding employee (likely EmployeeId conflict). EmployeeId: {EmployeeId}", employee.EmployeeId);
            throw new ConflictException($"An employee with ID {employee.EmployeeId} might already exist or another unique constraint was violated.");
        }
    }

    public async Task<DimEmployee> GetEmployeeByIdAsync(int employeeId)
    {
        _logger.LogDebug("MongoRepo: Getting employee by EmployeeId: {EmployeeId}", employeeId);
        var filter = Builders<DimEmployeeMongoDocument>.Filter.Eq(doc => doc.EmployeeId, employeeId);
        var document = await _employeesCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Employee with EmployeeId {EmployeeId} not found.", employeeId);
            throw new NotFoundException($"Employee with ID {employeeId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all employees.");
        var documents = await _employeesCollection.Find(FilterDefinition<DimEmployeeMongoDocument>.Empty)
                                                 .Sort(Builders<DimEmployeeMongoDocument>.Sort.Ascending(x => x.EmployeeId)) // Or by another field
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateEmployeeAsync(DimEmployee employee)
    {
        _logger.LogInformation("MongoRepo: Attempting to update employee with EmployeeId: {EmployeeId}", employee.EmployeeId);
        var filter = Builders<DimEmployeeMongoDocument>.Filter.Eq(doc => doc.EmployeeId, employee.EmployeeId);
        
        var existingDocument = await _employeesCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
             _logger.LogWarning("MongoRepo: EmployeeId {Id} not found for update.", employee.EmployeeId);
            throw new NotFoundException($"Employee with ID {employee.EmployeeId} not found for update.");
        }
        
        existingDocument.BirthDate = employee.BirthDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        existingDocument.CareerStartDate = employee.CareerStartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        try
        {
            var result = await _employeesCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: EmployeeId {Id} not matched during ReplaceOne operation.", employee.EmployeeId);
                throw new NotFoundException($"Employee with ID {employee.EmployeeId} not found for update (concurrent modification?).");
            }
             _logger.LogInformation("MongoRepo: Employee with EmployeeId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   employee.EmployeeId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // This should not happen if we are updating by EmployeeId and EmployeeId is unique.
            // Could happen if some other field being updated violates a different unique index.
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating employee EmployeeId {Id}.", employee.EmployeeId);
            throw new ConflictException("Updating employee record resulted in a conflict.");
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete employee with EmployeeId: {EmployeeId}", employeeId);
        var filter = Builders<DimEmployeeMongoDocument>.Filter.Eq(doc => doc.EmployeeId, employeeId);
        var result = await _employeesCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Employee with EmployeeId {EmployeeId} not found for deletion.", employeeId);
            throw new NotFoundException($"Employee with ID {employeeId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Employee with EmployeeId {EmployeeId} deleted. Count: {DeletedCount}", 
                               employeeId, result.DeletedCount);
    }
}