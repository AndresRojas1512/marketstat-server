namespace MarketStat.Contracts.Dimensions.DimEmployer;

public interface ISubmitDimEmployerCommand
{
    string EmployerName { get; }
    string Inn { get; }
    string Ogrn { get; }
    string Kpp { get; }
    DateOnly RegistrationDate { get; }
    string LegalAddress { get; }
    string ContactEmail { get; }
    string ContactPhone { get; }
    int IndustryFieldId { get; }
}