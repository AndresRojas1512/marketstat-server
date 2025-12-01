namespace MarketStat.Common.Dto.Dimensions.DimEmployee;

using System.ComponentModel.DataAnnotations;

public class PartialUpdateDimEmployeeDto
{
    [StringLength(255)]
    public string? EmployeeRefId { get; set; }

    public DateOnly? CareerStartDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? EducationId { get; set; }

    [Range(1900, 2100)]
    public short? GraduationYear { get; set; }
}
