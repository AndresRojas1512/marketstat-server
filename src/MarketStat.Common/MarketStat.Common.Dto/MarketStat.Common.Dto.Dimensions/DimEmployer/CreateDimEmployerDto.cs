using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

public class CreateDimEmployerDto
{
    [Required(ErrorMessage = "Employer name is required.")]
    [StringLength(255, ErrorMessage = "Employer name cannot exceed 255 characters.")]
    public string EmployerName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "INN is required.")]
    [StringLength(12, MinimumLength = 10, ErrorMessage = "INN must be 10 or 12 characters.")]
    // [RegularExpression("^[0-9]{10}([0-9]{2})?$", ErrorMessage = "Invalid INN format.")]
    public string Inn { get; set; } = string.Empty;

    [Required(ErrorMessage = "OGRN is required.")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "OGRN must be 13 characters.")]
    // [RegularExpression("^[0-9]{13}$", ErrorMessage = "Invalid OGRN format.")]
    public string Ogrn { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "KPP is required.")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "KPP must be 9 characters.")]
    // [RegularExpression("^[0-9]{4}[0-9A-Z]{2}[0-9]{3}$", ErrorMessage = "Invalid KPP format.")]
    public string Kpp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Registration date is required.")]
    public DateOnly RegistrationDate { get; set; }
    
    [Required(ErrorMessage = "Legal address is required.")]
    public string LegalAddress { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Website is required.")]
    [StringLength(255, ErrorMessage = "Website URL cannot exceed 255 characters.")]
    [Url(ErrorMessage = "Invalid website URL format.")]
    public string Website { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact email is required.")]
    [StringLength(255, ErrorMessage = "Contact email cannot exceed 255 characters.")]
    [EmailAddress(ErrorMessage = "Invalid contact email format.")]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact phone is required.")]
    [StringLength(50, ErrorMessage = "Contact phone cannot exceed 50 characters.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string ContactPhone { get; set; } = string.Empty;
}