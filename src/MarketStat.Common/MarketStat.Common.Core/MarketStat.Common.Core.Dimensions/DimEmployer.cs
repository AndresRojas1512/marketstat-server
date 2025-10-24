using MarketStat.Common.Core.MarketStat.Common.Core.Facts;

namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployer
{
    public int EmployerId { get; set; }
    public string EmployerName { get; set; }
    public string Inn { get; set; } 
    public string Ogrn { get; set; } 
    public string Kpp { get; set; } 
    public DateOnly RegistrationDate { get; set; } 
    public string LegalAddress { get; set; } 
    public string ContactEmail { get; set; } 
    public string ContactPhone { get; set; } 
    public int IndustryFieldId { get; set; } 
    
    public virtual DimIndustryField? DimIndustryField { get; set; } 
    public virtual ICollection<FactSalary> FactSalaries { get; set; } 

    public DimEmployer()
    {
        EmployerName = string.Empty;
        Inn = string.Empty;
        Ogrn = string.Empty;
        Kpp = string.Empty;
        LegalAddress = string.Empty;
        ContactEmail = string.Empty;
        ContactPhone = string.Empty;
        FactSalaries = new List<FactSalary>();
    }
    
    public DimEmployer(
        int employerId, 
        string employerName, 
        string inn, 
        string ogrn, 
        string kpp,
        DateOnly registrationDate, 
        string legalAddress, 
        string contactEmail, 
        string contactPhone,
        int industryFieldId)
    {
        EmployerId = employerId;
        EmployerName = employerName ?? throw new ArgumentNullException(nameof(employerName));
        Inn = inn ?? throw new ArgumentNullException(nameof(inn));
        Ogrn = ogrn ?? throw new ArgumentNullException(nameof(ogrn));
        Kpp = kpp ?? throw new ArgumentNullException(nameof(kpp));
        RegistrationDate = registrationDate;
        LegalAddress = legalAddress ?? throw new ArgumentNullException(nameof(legalAddress));
        ContactEmail = contactEmail ?? throw new ArgumentNullException(nameof(contactEmail));
        ContactPhone = contactPhone ?? throw new ArgumentNullException(nameof(contactPhone));
        IndustryFieldId = industryFieldId;
            
        FactSalaries = new List<FactSalary>();
    }
}