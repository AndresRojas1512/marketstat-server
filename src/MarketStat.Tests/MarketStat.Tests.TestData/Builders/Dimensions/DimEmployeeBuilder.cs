namespace MarketStat.Tests.TestData.Builders.Dimensions;

using MarketStat.Common.Core.Dimensions;

public class DimEmployeeBuilder
{
    private int _employeeId;
    private string _employeeRefId = "test-ref-123";
    private DateOnly _birthDate = new(1990, 1, 1);
    private DateOnly _careerStartDate = new(2015, 6, 1);
    private string? _gender = "Male";
    private int? _educationId = 1;
    private short? _graduationYear = 2014;

    public DimEmployeeBuilder WithId(int id)
    {
        _employeeId = id;
        return this;
    }

    public DimEmployeeBuilder WithEmployeeRefId(string refId)
    {
        _employeeRefId = refId;
        return this;
    }

    public DimEmployeeBuilder WithBirthDate(DateOnly birthDate)
    {
        _birthDate = birthDate;
        return this;
    }

    public DimEmployeeBuilder WithCareerStartDate(DateOnly careerStartDate)
    {
        _careerStartDate = careerStartDate;
        return this;
    }

    public DimEmployeeBuilder WithGender(string? gender)
    {
        _gender = gender;
        return this;
    }

    public DimEmployeeBuilder WithEducationId(int? educationId)
    {
        _educationId = educationId;
        return this;
    }

    public DimEmployeeBuilder WithGraduationYear(short? graduationYear)
    {
        _graduationYear = graduationYear;
        return this;
    }

    public DimEmployee Build()
    {
        return new DimEmployee(
            _employeeId,
            _employeeRefId,
            _birthDate,
            _careerStartDate,
            _gender,
            _educationId,
            _graduationYear);
    }
}
