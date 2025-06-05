using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Facts;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Facts;

public class StagedSalaryRecordMongoDocument 
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
        
    [BsonElement("recorded_date_text")]
    public string? RecordedDateText { get; set; }
    [BsonElement("city_name")]
    public string? CityName { get; set; }
    [BsonElement("oblast_name")]
    public string? OblastName { get; set; }
    [BsonElement("employer_name")]
    public string? EmployerName { get; set; }
    [BsonElement("standard_job_role_title")]
    public string? StandardJobRoleTitle { get; set; }
    [BsonElement("job_role_title")]
    public string? JobRoleTitle { get; set; }
    [BsonElement("hierarchy_level_name")]
    public string? HierarchyLevelName { get; set; }
    [BsonElement("employee_birth_date_text")]
    public string? EmployeeBirthDateText { get; set; }
    [BsonElement("employee_career_start_date_text")]
    public string? EmployeeCareerStartDateText { get; set; }
    [BsonElement("salary_amount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? SalaryAmount { get; set; }
    [BsonElement("bonus_amount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? BonusAmount { get; set; }
}

public class MongoFactSalaryRepository : IFactSalaryRepository
{
    private readonly IMongoCollection<FactSalaryMongoDocument> _factSalariesCollection;
    private readonly IMongoCollection<StagedSalaryRecordMongoDocument> _stagingCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoFactSalaryRepository> _logger;
    private readonly IMongoDatabase _database;

    public MongoFactSalaryRepository(IMongoDatabase database, ILogger<MongoFactSalaryRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _database = database ?? throw new ArgumentNullException(nameof(database));
        
        _factSalariesCollection = database.GetCollection<FactSalaryMongoDocument>("fact_salaries");
        _stagingCollection = database.GetCollection<StagedSalaryRecordMongoDocument>("api_fact_uploads_staging_mongo"); // Use a distinct name
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var salaryFactIdIndex = Builders<FactSalaryMongoDocument>.IndexKeys.Ascending(x => x.SalaryFactId);
        await _factSalariesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<FactSalaryMongoDocument>(salaryFactIdIndex, new CreateIndexOptions { Unique = true, Name = "idx_salary_fact_id_unique" })
        );
        await _factSalariesCollection.Indexes.CreateOneAsync(new CreateIndexModel<FactSalaryMongoDocument>(Builders<FactSalaryMongoDocument>.IndexKeys.Ascending(x => x.DateId), new CreateIndexOptions { Name = "idx_fs_date_id" }));
        await _factSalariesCollection.Indexes.CreateOneAsync(new CreateIndexModel<FactSalaryMongoDocument>(Builders<FactSalaryMongoDocument>.IndexKeys.Ascending(x => x.CityId), new CreateIndexOptions { Name = "idx_fs_city_id" }));
        await _factSalariesCollection.Indexes.CreateOneAsync(new CreateIndexModel<FactSalaryMongoDocument>(Builders<FactSalaryMongoDocument>.IndexKeys.Ascending(x => x.EmployerId), new CreateIndexOptions { Name = "idx_fs_employer_id" }));
        await _factSalariesCollection.Indexes.CreateOneAsync(new CreateIndexModel<FactSalaryMongoDocument>(Builders<FactSalaryMongoDocument>.IndexKeys.Ascending(x => x.JobRoleId), new CreateIndexOptions { Name = "idx_fs_job_role_id" }));
        await _factSalariesCollection.Indexes.CreateOneAsync(new CreateIndexModel<FactSalaryMongoDocument>(Builders<FactSalaryMongoDocument>.IndexKeys.Ascending(x => x.EmployeeId), new CreateIndexOptions { Name = "idx_fs_employee_id" }));
        _logger.LogInformation("Ensured indexes for 'fact_salaries' collection.");
    }

    private FactSalary ToDomain(FactSalaryMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new FactSalary
        {
            SalaryFactId = doc.SalaryFactId,
            DateId = doc.DateId,
            CityId = doc.CityId,
            EmployerId = doc.EmployerId,
            JobRoleId = doc.JobRoleId,
            EmployeeId = doc.EmployeeId,
            SalaryAmount = doc.SalaryAmount,
            BonusAmount = doc.BonusAmount
        };
    }

    private FactSalaryMongoDocument FromDomain(FactSalary domain)
    {
        if (domain == null) return null!;
        return new FactSalaryMongoDocument
        {
            SalaryFactId = domain.SalaryFactId,
            DateId = domain.DateId,
            CityId = domain.CityId,
            EmployerId = domain.EmployerId,
            JobRoleId = domain.JobRoleId,
            EmployeeId = domain.EmployeeId,
            SalaryAmount = domain.SalaryAmount,
            BonusAmount = domain.BonusAmount
        };
    }
    private StagedSalaryRecordMongoDocument FromStagingDto(StagedSalaryRecordDto dto)
    {
         return new StagedSalaryRecordMongoDocument
        {
            RecordedDateText = dto.RecordedDateText, CityName = dto.CityName, OblastName = dto.OblastName,
            EmployerName = dto.EmployerName, StandardJobRoleTitle = dto.StandardJobRoleTitle,
            JobRoleTitle = dto.JobRoleTitle, HierarchyLevelName = dto.HierarchyLevelName,
            EmployeeBirthDateText = dto.EmployeeBirthDateText, EmployeeCareerStartDateText = dto.EmployeeCareerStartDateText,
            SalaryAmount = dto.SalaryAmount, BonusAmount = dto.BonusAmount
        };
    }


    // --- CRUD Implementations ---
    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        _logger.LogInformation("MongoRepo: Adding FactSalary with potential SalaryFactId: {SalaryFactId}", salary.SalaryFactId);
        if (salary.SalaryFactId == 0)
        {
            salary.SalaryFactId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "salary_fact_id");
            _logger.LogInformation("MongoRepo: Generated new SalaryFactId {GeneratedId}", salary.SalaryFactId);
        }
        var document = FromDomain(salary);
        try
        {
            await _factSalariesCollection.InsertOneAsync(document);
             _logger.LogInformation("MongoRepo: Added FactSalary with SalaryFactId {SalaryFactId}, ObjectId {ObjectId}", document.SalaryFactId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding FactSalary with SalaryFactId {SalaryFactId}", salary.SalaryFactId);
            throw new ConflictException($"FactSalary with ID {salary.SalaryFactId} might already exist.");
        }
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(long salaryFactId)
    {
        _logger.LogDebug("MongoRepo: Getting FactSalary by SalaryFactId: {SalaryFactId}", salaryFactId);
        var filter = Builders<FactSalaryMongoDocument>.Filter.Eq(doc => doc.SalaryFactId, salaryFactId);
        var document = await _factSalariesCollection.Find(filter).FirstOrDefaultAsync();
        if (document == null) throw new NotFoundException($"FactSalary with ID {salaryFactId} not found.");
        return ToDomain(document);
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all FactSalaries.");
        var documents = await _factSalariesCollection.Find(FilterDefinition<FactSalaryMongoDocument>.Empty).ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        _logger.LogInformation("MongoRepo: Updating FactSalary with SalaryFactId: {SalaryFactId}", salaryFact.SalaryFactId);
        var filter = Builders<FactSalaryMongoDocument>.Filter.Eq(doc => doc.SalaryFactId, salaryFact.SalaryFactId);
        var documentToUpdate = FromDomain(salaryFact); // Create full document for replacement

        // To preserve the original MongoDB _id, we should use an update definition or fetch then replace.
        // For simplicity of this example, if we use ReplaceOne with a new FromDomain object,
        // ensure the _id is not part of FromDomain or handle it carefully if it were.
        // Better: use UpdateOne with $set.
        var updateDef = Builders<FactSalaryMongoDocument>.Update
            .Set(doc => doc.DateId, salaryFact.DateId)
            .Set(doc => doc.CityId, salaryFact.CityId)
            .Set(doc => doc.EmployerId, salaryFact.EmployerId)
            .Set(doc => doc.JobRoleId, salaryFact.JobRoleId)
            .Set(doc => doc.EmployeeId, salaryFact.EmployeeId)
            .Set(doc => doc.SalaryAmount, salaryFact.SalaryAmount)
            .Set(doc => doc.BonusAmount, salaryFact.BonusAmount);
        
        var result = await _factSalariesCollection.UpdateOneAsync(filter, updateDef);
        if (result.MatchedCount == 0) throw new NotFoundException($"FactSalary with ID {salaryFact.SalaryFactId} not found for update.");
        _logger.LogInformation("MongoRepo: Updated FactSalary {SalaryFactId}. Matched: {M}, Modified: {Mod}", salaryFact.SalaryFactId, result.MatchedCount, result.ModifiedCount);
    }

    public async Task DeleteFactSalaryByIdAsync(long salaryFactId)
    {
        _logger.LogInformation("MongoRepo: Deleting FactSalary with SalaryFactId: {SalaryFactId}", salaryFactId);
        var filter = Builders<FactSalaryMongoDocument>.Filter.Eq(doc => doc.SalaryFactId, salaryFactId);
        var result = await _factSalariesCollection.DeleteOneAsync(filter);
        if (result.DeletedCount == 0) throw new NotFoundException($"FactSalary with ID {salaryFactId} not found for deletion.");
        _logger.LogInformation("MongoRepo: Deleted FactSalary {SalaryFactId}. Count: {DelCount}", salaryFactId, result.DeletedCount);
    }

    // --- ETL-RELATED METHODS for MongoDB ---
    public async Task TruncateStagingTableAsync(string stagingCollectionName) // Parameter is now collection name
    {
        _logger.LogInformation("MongoRepo: Clearing staging collection: {StagingCollectionName}", stagingCollectionName);
        var collection = _database.GetCollection<BsonDocument>(stagingCollectionName); // Use BsonDocument if schema varies or for generic truncate
        await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        _logger.LogInformation("MongoRepo: Staging collection {StagingCollectionName} cleared.", stagingCollectionName);
    }

    public async Task BatchInsertToStagingTableAsync(string stagingCollectionName, IEnumerable<StagedSalaryRecordDto> records)
    {
        if (records == null || !records.Any())
        {
            _logger.LogInformation("MongoRepo: No records for batch insert into {StagingCollectionName}.", stagingCollectionName);
            return;
        }
        _logger.LogInformation("MongoRepo: Starting batch insert of {RecordCount} records into {StagingCollectionName}.", records.Count(), stagingCollectionName);
        var collection = _database.GetCollection<StagedSalaryRecordMongoDocument>(stagingCollectionName);
        var mongoDocs = records.Select(FromStagingDto); // Convert DTOs to Mongo documents
        await collection.InsertManyAsync(mongoDocs);
        _logger.LogInformation("MongoRepo: Batch insert of {RecordCount} records into {StagingCollectionName} completed.", mongoDocs.Count());
    }

    public Task<(int insertedCount, int skippedCount)> CallBulkLoadFromStagingProcedureAsync(string stagingTableName)
    {
        _logger.LogWarning("MongoRepo: CallBulkLoadFromStagingProcedureAsync is a PostgreSQL-specific concept and not directly applicable to MongoDB ETL flow. The core ETL logic should be in the service layer for MongoDB.");
        throw new NotImplementedException("MongoDB ETL logic is handled in the service layer, not by calling a PG stored procedure.");
    }


    // --- Analytical Methods
    public Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        _logger.LogWarning("MongoRepo: GetFactSalariesByFilterAsync (fn_filtered_salaries equivalent) not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<string?> GetBenchmarkingReportJsonAsync(BenchmarkQueryDto filters)
    {
        _logger.LogWarning("MongoRepo: GetBenchmarkingReportJsonAsync (fn_compute_benchmark_data equivalent) not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        _logger.LogWarning("MongoRepo: GetSalaryDistributionAsync (fn_salary_distribution equivalent) not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        _logger.LogWarning("MongoRepo: GetSalarySummaryAsync (fn_salary_summary equivalent) not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        _logger.LogWarning("MongoRepo: GetSalaryTimeSeriesAsync (fn_salary_time_series equivalent) not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto)
    {
        _logger.LogWarning("MongoRepo: GetPublicRolesByLocationIndustryAsync not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(PublicSalaryByEducationQueryDto queryDto)
    {
        _logger.LogWarning("MongoRepo: GetPublicSalaryByEducationInIndustryAsync not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
    public Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(PublicTopEmployerRoleSalariesQueryDto queryDto)
    {
        _logger.LogWarning("MongoRepo: GetPublicTopEmployerRoleSalariesInIndustryAsync not implemented for MongoDB.");
        throw new NotImplementedException("Analytical function not implemented for MongoDB.");
    }
}