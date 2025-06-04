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
    public string Website { get; set; } 
    public string ContactEmail { get; set; } 
    public string ContactPhone { get; set; }
    
    public virtual ICollection<DimEmployerIndustryField> EmployerIndustryFields { get; set; }
    public virtual ICollection<FactSalary> FactSalaries { get; set; }

    public DimEmployer()
    {
        EmployerName = string.Empty;
        Inn = string.Empty;
        Ogrn = string.Empty;
        Kpp = string.Empty;
        LegalAddress = string.Empty;
        Website = string.Empty;
        ContactEmail = string.Empty;
        ContactPhone = string.Empty;
        EmployerIndustryFields = new List<DimEmployerIndustryField>();
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
        string website, 
        string contactEmail, 
        string contactPhone)
    {
        EmployerId = employerId;
        EmployerName = employerName ?? throw new ArgumentNullException(nameof(employerName));
        Inn = inn ?? throw new ArgumentNullException(nameof(inn));
        Ogrn = ogrn ?? throw new ArgumentNullException(nameof(ogrn));
        Kpp = kpp ?? throw new ArgumentNullException(nameof(kpp));
        RegistrationDate = registrationDate;
        LegalAddress = legalAddress ?? throw new ArgumentNullException(nameof(legalAddress));
        Website = website ?? throw new ArgumentNullException(nameof(website));
        ContactEmail = contactEmail ?? throw new ArgumentNullException(nameof(contactEmail));
        ContactPhone = contactPhone ?? throw new ArgumentNullException(nameof(contactPhone));
            
        EmployerIndustryFields = new List<DimEmployerIndustryField>();
        FactSalaries = new List<FactSalary>();
    }
}