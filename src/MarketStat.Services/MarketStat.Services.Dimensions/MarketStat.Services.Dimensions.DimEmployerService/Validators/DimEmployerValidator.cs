namespace MarketStat.Services.Dimensions.DimEmployerService.Validators;
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
        string website, 
        string contactEmail, 
        string contactPhone)
    {
        if (string.IsNullOrWhiteSpace(employerName))
            throw new ArgumentException("Employer name is required.", nameof(employerName));
        if (employerName.Length > MaxNameLength)
            throw new ArgumentException($"Employer name must be {MaxNameLength} characters or fewer.", nameof(employerName));

        if (string.IsNullOrWhiteSpace(inn))
            throw new ArgumentException("INN is required.", nameof(inn));
        if (inn.Length < MinInnLength || inn.Length > MaxInnLength)
            throw new ArgumentException($"INN must be between {MinInnLength} and {MaxInnLength} characters.", nameof(inn));
        // TODO: Add Regex for INN format if needed: e.g., ^[0-9]{10}([0-9]{2})?$

        if (string.IsNullOrWhiteSpace(ogrn))
            throw new ArgumentException("OGRN is required.", nameof(ogrn));
        if (ogrn.Length != MaxOgrnLength)
            throw new ArgumentException($"OGRN must be {MaxOgrnLength} characters.", nameof(ogrn));
        // TODO: Add Regex for OGRN format if needed: e.g., ^[0-9]{13}$

        if (string.IsNullOrWhiteSpace(kpp))
            throw new ArgumentException("KPP is required.", nameof(kpp));
        if (kpp.Length != MaxKppLength)
            throw new ArgumentException($"KPP must be {MaxKppLength} characters.", nameof(kpp));
        // TODO: Add Regex for KPP format if needed

        if (registrationDate == default || registrationDate > DateOnly.FromDateTime(DateTime.UtcNow)) // Check for default and future date
            throw new ArgumentException("Registration date must be a valid past or present date.", nameof(registrationDate));
        
        if (string.IsNullOrWhiteSpace(legalAddress))
            throw new ArgumentException("Legal address is required.", nameof(legalAddress));

        if (string.IsNullOrWhiteSpace(website))
            throw new ArgumentException("Website is required.", nameof(website));
        if (website.Length > MaxWebsiteLength)
            throw new ArgumentException($"Website URL must be {MaxWebsiteLength} characters or fewer.", nameof(website));
        if (!Uri.TryCreate(website, UriKind.Absolute, out _))
             throw new ArgumentException("Invalid website URL format.", nameof(website));


        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new ArgumentException("Contact email is required.", nameof(contactEmail));
        if (contactEmail.Length > MaxEmailLength)
            throw new ArgumentException($"Contact email must be {MaxEmailLength} characters or fewer.", nameof(contactEmail));
        // if (!Regex.IsMatch(contactEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        //      throw new ArgumentException("Invalid contact email format.", nameof(contactEmail));


        if (string.IsNullOrWhiteSpace(contactPhone))
            throw new ArgumentException("Contact phone is required.", nameof(contactPhone));
        if (contactPhone.Length > MaxPhoneLength)
            throw new ArgumentException($"Contact phone must be {MaxPhoneLength} characters or fewer.", nameof(contactPhone));
        // TODO: Add Regex for phone format if needed
    }

    public static void ValidateForUpdate(
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
        if (employerId <= 0)
            throw new ArgumentException("EmployerId must be a positive integer for update.", nameof(employerId));
        
        ValidateForCreate(employerName, inn, ogrn, kpp, registrationDate, legalAddress, website, contactEmail, contactPhone);
    }
}