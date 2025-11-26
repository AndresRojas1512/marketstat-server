namespace MarketStat.Contracts.Dimensions.DimEmployee;

public interface IGetDimEmployeeResponse
{
    int EmployeeId { get; }
    string EmployeeRefId { get; }
    DateOnly BirthDate { get; }
    DateOnly CareerStartDate { get; }
    string? Gender { get; }
    int? EducationId { get; }
    short? GraduationYear { get; }
}