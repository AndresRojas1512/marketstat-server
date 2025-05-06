namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;

public record DimEducationDto(
    int EducationId,
    string Specialty,
    string SpecialtyCode,
    int EducationLevelId,
    int IndustryFieldId
);