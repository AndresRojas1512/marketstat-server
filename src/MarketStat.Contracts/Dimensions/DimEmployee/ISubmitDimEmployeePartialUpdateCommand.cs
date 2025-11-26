namespace MarketStat.Contracts.Dimensions.DimEmployee;

public interface ISubmitDimEmployeePartialUpdateCommand
{
    int EmployeeId { get; }
    string? EmployeeRefId { get; }
    DateOnly? CareerStartDate { get; }
    int? EducationId { get; }
    short? GraduationYear { get; }
}