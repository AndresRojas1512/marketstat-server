namespace MarketStat.Common.Validators.Dimensions;
public static class DimEmployerValidator
{
    private const int MaxNameLength = 255;
    private const int MaxInnLength = 12;
    private const int MinInnLength = 10;
    private const int MaxOgrnLength = 13;
    private const int MaxKppLength = 9;
    private const int MaxWebsiteLength = 255;
    private const int MaxEmailLength = 255;
    private const int MaxPhoneLength = 50;

    public static void ValidateForCreate(
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
        if (string.IsNullOrWhiteSpace(employerName))
            throw new ArgumentException("Employer name is required.", nameof(employerName));
        if (employerName.Length > MaxNameLength)
            throw new ArgumentException($"Employer name must be {MaxNameLength} characters or fewer.", nameof(employerName));

        if (string.IsNullOrWhiteSpace(inn))
            throw new ArgumentException("INN is required.", nameof(inn));
        if (inn.Length < MinInnLength || inn.Length > MaxInnLength)
            throw new ArgumentException($"INN must be between {MinInnLength} and {MaxInnLength} characters.", nameof(inn));

        if (string.IsNullOrWhiteSpace(ogrn))
            throw new ArgumentException("OGRN is required.", nameof(ogrn));
        if (ogrn.Length != MaxOgrnLength)
            throw new ArgumentException($"OGRN must be {MaxOgrnLength} characters.", nameof(ogrn));

        if (string.IsNullOrWhiteSpace(kpp))
            throw new ArgumentException("KPP is required.", nameof(kpp));
        if (kpp.Length != MaxKppLength)
            throw new ArgumentException($"KPP must be {MaxKppLength} characters.", nameof(kpp));

        if (registrationDate == default || registrationDate > DateOnly.FromDateTime(DateTime.UtcNow)) // Check for default and future date
            throw new ArgumentException("Registration date must be a valid past or present date.", nameof(registrationDate));
        
        if (string.IsNullOrWhiteSpace(legalAddress))
            throw new ArgumentException("Legal address is required.", nameof(legalAddress));

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new ArgumentException("Contact email is required.", nameof(contactEmail));
        if (contactEmail.Length > MaxEmailLength)
            throw new ArgumentException($"Contact email must be {MaxEmailLength} characters or fewer.", nameof(contactEmail));
        

        if (string.IsNullOrWhiteSpace(contactPhone))
            throw new ArgumentException("Contact phone is required.", nameof(contactPhone));
        if (contactPhone.Length > MaxPhoneLength)
            throw new ArgumentException($"Contact phone must be {MaxPhoneLength} characters or fewer.", nameof(contactPhone));

        if (industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(industryFieldId));
    }

    public static void ValidateForUpdate(
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
        if (employerId <= 0)
            throw new ArgumentException("EmployerId must be a positive integer for update.", nameof(employerId));
        
        ValidateForCreate(employerName, inn, ogrn, kpp, registrationDate, legalAddress, contactEmail, contactPhone, industryFieldId);
    }
}
