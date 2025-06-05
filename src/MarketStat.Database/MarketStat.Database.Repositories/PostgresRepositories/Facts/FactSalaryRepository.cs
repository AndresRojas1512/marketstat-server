using System.Data;
using System.Text;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly MarketStatDbContext _dbContext;
    private readonly ILogger<FactSalaryRepository> _logger;

    public FactSalaryRepository(MarketStatDbContext dbContext, ILogger<FactSalaryRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        var dbModel = FactSalaryConverter.ToDbModel(salary);
        await _dbContext.FactSalaries.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found when adding salary fact.");
        }
        salary.SalaryFactId = dbModel.SalaryFactId;
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(long salaryId)
    {
        var dbSalary = await _dbContext.FactSalaries
            .AsNoTracking()
            .FirstOrDefaultAsync(fs => fs.SalaryFactId == salaryId);

        if (dbSalary == null)
        {
            throw new NotFoundException($"Salary fact with ID {salaryId} not found.");
        }
        return FactSalaryConverter.ToDomain(dbSalary);
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var allDbModels = await _dbContext.FactSalaries.AsNoTracking().ToListAsync();
        return allDbModels.Select(FactSalaryConverter.ToDomain);
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Repository: GetFactSalariesByFilterAsync called with filter: {@FilterDto}", filterDto);
        
        _logger.LogDebug(
            "Repository: Parameters for fn_filtered_salaries - IndustryFieldId: {IndustryFieldId}, " +
            "StandardJobRoleId: {StandardJobRoleId}, HierarchyLevelId: {HierarchyLevelId}, DistrictId: {DistrictId}, " +
            "OblastId: {OblastId}, CityId: {CityId}, DateStart: {DateStart}, DateEnd: {DateEnd}",
            filterDto.IndustryFieldId, filterDto.StandardJobRoleId, filterDto.HierarchyLevelId,
            filterDto.DistrictId, filterDto.OblastId, filterDto.CityId,
            filterDto.DateStart, filterDto.DateEnd);

        try
        {
            var results = await _dbContext.Set<FactSalary>()
                .FromSqlInterpolated(
                    @$"SELECT 
                          f.salary_fact_id, 
                          f.date_id, 
                          f.city_id_from_fact         AS city_id, 
                          f.employer_id, 
                          f.job_role_id_from_fact     AS job_role_id, 
                          f.employee_id, 
                          f.salary_amount, 
                          f.bonus_amount 
                      FROM marketstat.fn_filtered_salaries(
                          {filterDto.IndustryFieldId}, {filterDto.StandardJobRoleId}, {filterDto.HierarchyLevelId}, 
                          {filterDto.DistrictId}, {filterDto.OblastId}, {filterDto.CityId}, 
                          {filterDto.DateStart}, {filterDto.DateEnd}) AS f"
                )
                .AsNoTracking()
                .ToListAsync();
            
            _logger.LogInformation("Repository: Successfully retrieved {Count} salary facts from fn_filtered_salaries.", results.Count);
            return results;
        }
        catch (NpgsqlException npgEx) 
        {
            if (npgEx is PostgresException pgEx_specific)
            {
                _logger.LogError(pgEx_specific, 
                    "Repository: PostgresException while executing fn_filtered_salaries for filter: {@FilterDto}. SQLSTATE: {SqlState}, Message: {MessageText}, Detail: {Detail}, Hint: {Hint}, Position: {Position}, Routine: {Routine}", 
                    filterDto, pgEx_specific.SqlState, pgEx_specific.MessageText, pgEx_specific.Detail, pgEx_specific.Hint, pgEx_specific.Position, pgEx_specific.Routine);
            }
            else
            {
                _logger.LogError(npgEx, 
                    "Repository: NpgsqlException (not Postgres-specific) while executing fn_filtered_salaries for filter: {@FilterDto}. Message: {Message}", 
                    filterDto, npgEx.Message);
            }
            throw new ApplicationException("A database error occurred while fetching filtered salaries.", npgEx);
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Repository: Generic error executing fn_filtered_salaries or materializing FactSalary results for filter: {@FilterDto}", filterDto);
            throw new ApplicationException("An error occurred while processing filtered salaries.", ex);
        }
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        var dbModel = await _dbContext.FactSalaries.FirstOrDefaultAsync(fs => fs.SalaryFactId == salaryFact.SalaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFact.SalaryFactId} not found for update.");

        dbModel.DateId = salaryFact.DateId;
        dbModel.CityId = salaryFact.CityId;
        dbModel.EmployerId = salaryFact.EmployerId;
        dbModel.JobRoleId = salaryFact.JobRoleId;
        dbModel.EmployeeId = salaryFact.EmployeeId;
        dbModel.SalaryAmount = salaryFact.SalaryAmount;
        dbModel.BonusAmount = salaryFact.BonusAmount;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found during update.");
        }
    }

    public async Task DeleteFactSalaryByIdAsync(long salaryFactId)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFactId} not found for deletion.");

        _dbContext.FactSalaries.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetBenchmarkingReportJsonAsync(BenchmarkQueryDto filters)
    {
        _logger.LogInformation("GetBenchmarkingReportJsonAsync called with filters: {@Filters}", filters);

        string granularityString = filters.Granularity.ToString().ToLowerInvariant();
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("p_industry_field_id", NpgsqlDbType.Integer) { Value = (object?)filters.IndustryFieldId ?? DBNull.Value },
            new NpgsqlParameter("p_standard_job_role_id", NpgsqlDbType.Integer) { Value = (object?)filters.StandardJobRoleId ?? DBNull.Value },
            new NpgsqlParameter("p_hierarchy_level_id", NpgsqlDbType.Integer) { Value = (object?)filters.HierarchyLevelId ?? DBNull.Value },
            new NpgsqlParameter("p_district_id", NpgsqlDbType.Integer) { Value = (object?)filters.DistrictId ?? DBNull.Value },
            new NpgsqlParameter("p_oblast_id", NpgsqlDbType.Integer) { Value = (object?)filters.OblastId ?? DBNull.Value },
            new NpgsqlParameter("p_city_id", NpgsqlDbType.Integer) { Value = (object?)filters.CityId ?? DBNull.Value },
            new NpgsqlParameter("p_date_start", NpgsqlDbType.Date) { Value = (object?)filters.DateStart ?? DBNull.Value },
            new NpgsqlParameter("p_date_end", NpgsqlDbType.Date) { Value = (object?)filters.DateEnd ?? DBNull.Value },
            new NpgsqlParameter("p_target_percentile", NpgsqlDbType.Integer) { Value = filters.TargetPercentile },
            new NpgsqlParameter("p_granularity", NpgsqlDbType.Text) { Value = granularityString },
            new NpgsqlParameter("p_periods", NpgsqlDbType.Integer) { Value = filters.Periods }
        };

        var sql = @"SELECT marketstat.fn_compute_benchmark_data(
                        p_industry_field_id            := @p_industry_field_id,
                        p_standard_job_role_id         := @p_standard_job_role_id,
                        p_hierarchy_level_id           := @p_hierarchy_level_id,
                        p_district_id                  := @p_district_id,
                        p_oblast_id                    := @p_oblast_id,
                        p_city_id                      := @p_city_id,
                        p_date_start                   := @p_date_start,
                        p_date_end                     := @p_date_end,
                        p_target_percentile            := @p_target_percentile,
                        p_granularity                  := @p_granularity,
                        p_periods                      := @p_periods
                    );";

        _logger.LogDebug("Executing SQL for benchmark report: {Sql}", sql);
        foreach (var p in parameters)
        {
            var paramValueForLog = (p.Value == DBNull.Value) ? "<DBNull>" : p.Value;
            _logger.LogDebug("Parameter: {ParameterName} = {ParameterValue} (NpgsqlDbType: {NpgsqlDbType})", p.ParameterName, paramValueForLog, p.NpgsqlDbType);
        }

        string? jsonResult = null;
        var connection = _dbContext.Database.GetDbConnection();
        var closeConnection = false;
        try
        {
            if (connection.State != ConnectionState.Open) { await connection.OpenAsync(); closeConnection = true; }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.AddRange(parameters.ToArray());
                var scalarResult = await command.ExecuteScalarAsync();
                if (scalarResult != null && scalarResult != DBNull.Value)
                {
                    jsonResult = scalarResult as string;
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error executing GetBenchmarkingReportJsonAsync SQL call.");
            throw;
        }
        finally
        {
            if (closeConnection && connection.State == ConnectionState.Open) { await connection.CloseAsync(); }
        }
        _logger.LogInformation("GetBenchmarkingReportJsonAsync raw JSON result: {JsonResult}", jsonResult);
        return jsonResult;
    }

    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        return await _dbContext.Set<SalaryDistributionBucketDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_distribution(p_source_temp_table_name := NULL, p_industry_field_id := {filters.IndustryFieldId}, p_standard_job_role_id := {filters.StandardJobRoleId}, p_hierarchy_level_id := {filters.HierarchyLevelId}, p_district_id := {filters.DistrictId}, p_oblast_id := {filters.OblastId}, p_city_id := {filters.CityId}, p_date_start := {filters.DateStart}, p_date_end := {filters.DateEnd})")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        return await _dbContext.Set<SalarySummaryDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_summary(p_source_temp_table_name := NULL, p_industry_field_id := {filters.IndustryFieldId}, p_standard_job_role_id := {filters.StandardJobRoleId}, p_hierarchy_level_id := {filters.HierarchyLevelId}, p_district_id := {filters.DistrictId}, p_oblast_id := {filters.OblastId}, p_city_id := {filters.CityId}, p_date_start := {filters.DateStart}, p_date_end := {filters.DateEnd}, p_target_percentile := {targetPercentile})")
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        string granularityString = granularity.ToString().ToLowerInvariant();
        return await _dbContext.Set<SalaryTimeSeriesPointDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_time_series(p_source_temp_table_name := NULL, p_industry_field_id := {filters.IndustryFieldId}, p_standard_job_role_id := {filters.StandardJobRoleId}, p_hierarchy_level_id := {filters.HierarchyLevelId}, p_district_id := {filters.DistrictId}, p_oblast_id := {filters.OblastId}, p_city_id := {filters.CityId}, p_filter_date_start := {filters.DateStart}, p_filter_date_end := {filters.DateEnd}, p_granularity := {granularityString}, p_periods := {periods})")
            .AsNoTracking()
            .ToListAsync();
    }
    
    // Public analytical methods
    
    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto)
    {
        _logger.LogInformation(
            "Repository: Calling fn_public_get_roles_by_location_industry with DTO: {@QueryDto}", queryDto);

        try
        {
            var results = await _dbContext.Set<PublicRoleByLocationIndustryDto>()
                .FromSqlInterpolated($"SELECT * FROM marketstat.fn_public_get_roles_by_location_industry({queryDto.IndustryFieldId}, {queryDto.FederalDistrictId}, {queryDto.OblastId}, {queryDto.CityId}, {queryDto.MinSalaryRecordsForRole})")
                .AsNoTracking()
                .ToListAsync();
                
            _logger.LogInformation("Repository: fn_public_get_roles_by_location_industry returned {Count} records.", results.Count);
            return results;
        }
        catch (NpgsqlException npgEx)
        {
            if (npgEx is PostgresException pgEx_specific)
            {
                _logger.LogError(pgEx_specific, "Repository: PostgresException executing fn_public_get_roles_by_location_industry with DTO {@QueryDto}. SQLSTATE: {SqlState}, Message: {MessageText}, Detail: {Detail}", 
                    queryDto, pgEx_specific.SqlState, pgEx_specific.MessageText, pgEx_specific.Detail);
            }
            else
            {
                _logger.LogError(npgEx, "Repository: NpgsqlException executing fn_public_get_roles_by_location_industry with DTO {@QueryDto}.", queryDto);
            }
            throw new ApplicationException("A database error occurred while fetching public roles by location/industry.", npgEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository: Generic error executing fn_public_get_roles_by_location_industry with DTO {@QueryDto}.", queryDto);
            throw new ApplicationException("An unexpected error occurred while fetching public roles by location/industry.", ex);
        }
    }
    
    // public async Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(PublicSalaryByEducationQueryDto queryDto)
    // {
    //     _logger.LogInformation(
    //         "Repository: Calling fn_public_salary_by_education_in_industry with DTO: {@QueryDto}", queryDto);
    //
    //     try
    //     {
    //         var results = await _dbContext.Set<PublicSalaryByEducationInIndustryDto>()
    //             .FromSqlInterpolated($"SELECT * FROM marketstat.fn_public_salary_by_education_in_industry({queryDto.IndustryFieldId}, {queryDto.TopNSpecialties}, {queryDto.MinEmployeesPerSpecialty}, {queryDto.MinEmployeesPerLevelInSpecialty})")
    //             .AsNoTracking()
    //             .ToListAsync();
    //         
    //         _logger.LogInformation("Repository: fn_public_salary_by_education_in_industry returned {Count} records.", results.Count);
    //         return results;
    //     }
    //     catch (NpgsqlException npgEx)
    //     {
    //         if (npgEx is PostgresException pgEx_specific)
    //         {
    //             _logger.LogError(pgEx_specific, "Repository: PostgresException executing fn_public_salary_by_education_in_industry with DTO {@QueryDto}. SQLSTATE: {SqlState}, Message: {MessageText}, Detail: {Detail}", 
    //                 queryDto, pgEx_specific.SqlState, pgEx_specific.MessageText, pgEx_specific.Detail);
    //         }
    //         else
    //         {
    //             _logger.LogError(npgEx, "Repository: NpgsqlException executing fn_public_salary_by_education_in_industry with DTO {@QueryDto}.", queryDto);
    //         }
    //         throw new ApplicationException("A database error occurred while fetching public salary by education data.", npgEx);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Repository: Generic error executing fn_public_salary_by_education_in_industry with DTO {@QueryDto}.", queryDto);
    //         throw new ApplicationException("An unexpected error occurred while fetching public salary by education data.", ex);
    //     }
    // }

    // public async Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
    //         PublicTopEmployerRoleSalariesQueryDto queryDto)
    // {
    //     _logger.LogInformation(
    //         "Repository: Calling fn_public_top_employer_role_salaries_in_industry with DTO: {@QueryDto}", queryDto);
    //
    //     try
    //     {
    //         var results = await _dbContext.Set<PublicTopEmployerRoleSalariesInIndustryDto>()
    //             .FromSqlInterpolated($"SELECT * FROM marketstat.fn_public_top_employer_role_salaries_in_industry({queryDto.IndustryFieldId}, {queryDto.TopNEmployers}, {queryDto.TopMRolesPerEmployer}, {queryDto.MinSalaryRecordsForRoleAtEmployer})")
    //             .AsNoTracking()
    //             .ToListAsync();
    //         
    //         _logger.LogInformation("Repository: fn_public_top_employer_role_salaries_in_industry returned {Count} records.", results.Count);
    //         return results;
    //     }
    //     catch (NpgsqlException npgEx)
    //     {
    //         if (npgEx is PostgresException pgEx_specific)
    //         {
    //             _logger.LogError(pgEx_specific, "Repository: PostgresException executing fn_public_top_employer_role_salaries_in_industry with DTO {@QueryDto}. SQLSTATE: {SqlState}, Message: {MessageText}, Detail: {Detail}", 
    //                 queryDto, pgEx_specific.SqlState, pgEx_specific.MessageText, pgEx_specific.Detail);
    //         }
    //         else
    //         {
    //             _logger.LogError(npgEx, "Repository: NpgsqlException executing fn_public_top_employer_role_salaries_in_industry with DTO {@QueryDto}.", queryDto);
    //         }
    //         throw new ApplicationException("A database error occurred while fetching top employer role salaries.", npgEx);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Repository: Generic error executing fn_public_top_employer_role_salaries_in_industry with DTO {@QueryDto}.", queryDto);
    //         throw new ApplicationException("An unexpected error occurred while fetching top employer role salaries.", ex);
    //     }
    // }
    
    
    // ETL methods

    // public async Task TruncateStagingTableAsync(string stagingTableName)
    // {
    //     _logger.LogInformation("Repository: Truncating staging table: {StagingTable}", stagingTableName);
    //     await _dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {stagingTableName};");
    //     _logger.LogInformation("Repository: Staging table {StagingTable} truncated", stagingTableName);
    // }
    
    // public async Task BatchInsertToStagingTableAsync(string stagingTableName, IEnumerable<StagedSalaryRecordDto> records)
    // {
    //     if (records == null || !records.Any())
    //     {
    //         _logger.LogInformation("Repository: No records provided for batch insert into {StagingTable}.", stagingTableName);
    //         return;
    //     }
    //
    //     _logger.LogInformation("Repository: Starting batch insert of {RecordCount} records into {StagingTable}.", records.Count(), stagingTableName);
    //
    //     var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection(); 
    //     
    //     if (connection.State != ConnectionState.Open)
    //     {
    //         _logger.LogWarning("[REPO BatchInsert] Connection was not open despite expecting an active transaction. Opening now.");
    //         await connection.OpenAsync(); 
    //     }
    //
    //     var copyCommand = $"COPY {stagingTableName} (recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount) FROM STDIN (FORMAT BINARY)";
    //
    //     NpgsqlBinaryImporter? writer = null;
    //     try
    //     {
    //         writer = await connection.BeginBinaryImportAsync(copyCommand);
    //         foreach (var record in records)
    //         {
    //             await writer.StartRowAsync();
    //             await writer.WriteAsync(record.RecordedDateText, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.CityName, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.OblastName, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.EmployerName, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.StandardJobRoleTitle, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.JobRoleTitle, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.HierarchyLevelName, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.EmployeeBirthDateText, NpgsqlDbType.Text);
    //             await writer.WriteAsync(record.EmployeeCareerStartDateText, NpgsqlDbType.Text);
    //             
    //             if (record.SalaryAmount.HasValue) await writer.WriteAsync(record.SalaryAmount.Value, NpgsqlDbType.Numeric);
    //             else await writer.WriteNullAsync();
    //             
    //             if (record.BonusAmount.HasValue) await writer.WriteAsync(record.BonusAmount.Value, NpgsqlDbType.Numeric);
    //             else await writer.WriteNullAsync();
    //         }
    //         await writer.CompleteAsync();
    //         _logger.LogInformation("Repository: Batch insert of {RecordCount} records into {StagingTable} completed.", records.Count(), stagingTableName);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Repository: Error during binary import to {StagingTable}.", stagingTableName);
    //         if (writer != null)
    //         {
    //             try { await writer.DisposeAsync(); }
    //             catch (Exception disposeEx) { _logger.LogError(disposeEx, "Repository: Error disposing NpgsqlBinaryImporter after an error."); }
    //         }
    //         throw;
    //     }
    //     finally
    //     {
    //         if (writer != null)
    //         {
    //             await writer.DisposeAsync();
    //         }
    //     }
    // }

    // public async Task<(int insertedCount, int skippedCount)> CallBulkLoadFromStagingProcedureAsync(string stagingTableNameFromService)
    // {
    //     string procedureParameterTableName = stagingTableNameFromService;
    //     if (procedureParameterTableName.StartsWith("marketstat.", StringComparison.OrdinalIgnoreCase))
    //     {
    //         procedureParameterTableName = procedureParameterTableName.Substring("marketstat.".Length);
    //         _logger.LogInformation("[REPO] Using non-schema-qualified table name for SP parameter: {TableName}", procedureParameterTableName);
    //     }
    //     else
    //     {
    //         _logger.LogInformation("[REPO] Using provided table name for SP parameter as is: {TableName}", procedureParameterTableName);
    //     }
    //
    //     _logger.LogInformation("[REPO] Calling SP: {SPName} with parameter table name: {ParameterTableName}", 
    //         "marketstat.bulk_load_salary_facts_from_staging", procedureParameterTableName);
    //     
    //     int insertedCount = 0;
    //     int skippedCount = 0;
    //
    //     NpgsqlConnection? connection = null;
    //     bool wasConnectionOpenedByThisMethod = false;
    //     
    //     try
    //     {
    //         connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
    //         if (connection.State != ConnectionState.Open)
    //         {
    //             await connection.OpenAsync();
    //             wasConnectionOpenedByThisMethod = true;
    //             _logger.LogDebug("[REPO] Connection was closed, opened it for SP call.");
    //         } else {
    //             _logger.LogDebug("[REPO] Connection was already open for SP call.");
    //         }
    //         
    //         IDbContextTransaction? efCoreTransaction = _dbContext.Database.CurrentTransaction;
    //         NpgsqlTransaction? npgsqlTransaction = (NpgsqlTransaction?)efCoreTransaction?.GetDbTransaction();
    //
    //         _logger.LogDebug("[REPO] Using EF Core Transaction ID: {TransactionId}, NpgsqlTransaction HashCode: {NpgsqlTransactionHashCode}", 
    //             efCoreTransaction?.TransactionId.ToString() ?? "None", 
    //             npgsqlTransaction?.GetHashCode().ToString() ?? "None (or not NpgsqlTransaction)");
    //
    //         await using var command = new NpgsqlCommand("marketstat.bulk_load_salary_facts_from_staging", connection)
    //         {
    //             CommandType = CommandType.StoredProcedure,
    //             Transaction = npgsqlTransaction 
    //         };
    //         
    //         // Add IN parameter
    //         command.Parameters.Add(new NpgsqlParameter("p_source_staging_table_name", NpgsqlDbType.Text) { Value = procedureParameterTableName });
    //         
    //         // Add OUT parameters - their names here must match the formal parameter names in the PG procedure
    //         var pInserted = new NpgsqlParameter("p_inserted_count", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output };
    //         var pSkipped = new NpgsqlParameter("p_skipped_count", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output };
    //         command.Parameters.Add(pInserted);
    //         command.Parameters.Add(pSkipped);
    //             
    //         _logger.LogInformation("[REPO] BEFORE ExecuteNonQueryAsync for SP call. Transaction assigned: {IsTransactionAssigned}", npgsqlTransaction != null);
    //         await command.ExecuteNonQueryAsync();
    //         _logger.LogInformation("[REPO] AFTER ExecuteNonQueryAsync for SP call - SUCCESS for staging table originally named: {OriginalStagingTableName}.", stagingTableNameFromService);
    //
    //         // Retrieve OUT parameter values
    //         insertedCount = (pInserted.Value != DBNull.Value && pInserted.Value != null) ? Convert.ToInt32(pInserted.Value) : 0;
    //         skippedCount = (pSkipped.Value != DBNull.Value && pSkipped.Value != null) ? Convert.ToInt32(pSkipped.Value) : 0;
    //
    //         _logger.LogInformation("[REPO] SP OUT Params - Inserted: {InsertedCount}, Skipped: {SkippedCount}", insertedCount, skippedCount);
    //     }
    //     catch(PostgresException pgEx)
    //     {
    //         _logger.LogError(pgEx, "[REPO] POSTGRES EXCEPTION executing SP {SPName} with input table {InputTable}. SQLState: {SqlState}, Message: {MessageText}, Detail: {Detail}", 
    //             "marketstat.bulk_load_salary_facts_from_staging", procedureParameterTableName, pgEx.SqlState, pgEx.MessageText, pgEx.Detail);
    //         throw new ApplicationException($"Database error during bulk load procedure execution: {pgEx.MessageText}", pgEx);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "[REPO] GENERIC EXCEPTION executing SP {SPName} with input table {InputTable}.", 
    //             "marketstat.bulk_load_salary_facts_from_staging", procedureParameterTableName);
    //         throw new ApplicationException("An unexpected error occurred while executing the bulk load procedure.", ex);
    //     }
    //     finally
    //     {
    //         if (wasConnectionOpenedByThisMethod && 
    //             _dbContext.Database.CurrentTransaction == null && // Only close if no encompassing EF transaction
    //             connection != null && 
    //             connection.State == ConnectionState.Open)
    //         {
    //             _logger.LogDebug("[REPO] Closing connection that was opened by this method as no EF transaction is active.");
    //             await connection.CloseAsync();
    //         } else if (connection != null) {
    //             _logger.LogDebug("[REPO] Leaving connection in state: {ConnectionState} as it's managed by EF transaction or was already open.", connection.State);
    //         }
    //     }
    //     return (insertedCount, skippedCount);
    // }
}