namespace MarketStat.Tests.TestData.Builders.Facts;

using MarketStat.Common.Core.Facts;

public class FactSalaryBuilder
{
    private long _salaryFactId;
    private int _dateId = 1;
    private int _locationId = 1;
    private int _employerId = 1;
    private int _jobId = 1;
    private int _employeeId = 1;
    private decimal _salaryAmount = 100000;

    public FactSalaryBuilder WithId(long id)
    {
        _salaryFactId = id;
        return this;
    }

    public FactSalaryBuilder WithDateId(int id)
    {
        _dateId = id;
        return this;
    }

    public FactSalaryBuilder WithLocationId(int id)
    {
        _locationId = id;
        return this;
    }

    public FactSalaryBuilder WithEmployerId(int id)
    {
        _employerId = id;
        return this;
    }

    public FactSalaryBuilder WithJobId(int id)
    {
        _jobId = id;
        return this;
    }

    public FactSalaryBuilder WithEmployeeId(int id)
    {
        _employeeId = id;
        return this;
    }

    public FactSalaryBuilder WithSalaryAmount(decimal amount)
    {
        _salaryAmount = amount;
        return this;
    }

    public FactSalary Build()
    {
        return new FactSalary(
            _salaryFactId,
            _dateId,
            _locationId,
            _employerId,
            _jobId,
            _employeeId,
            _salaryAmount);
    }
}
