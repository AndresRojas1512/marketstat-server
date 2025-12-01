namespace MarketStat.Common.Dto.Dimensions;

using System.ComponentModel.DataAnnotations;

public class CreateDimEmployerDto
{
    [Required(ErrorMessage = "Employer name is required.")]
    [StringLength(255, ErrorMessage = "Employer name cannot exceed 255 characters.")]
    public string EmployerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "INN is required.")]
    [StringLength(12, MinimumLength = 10, ErrorMessage = "INN must be 10 or 12 characters.")]
    public string Inn { get; set; } = string.Empty;

    [Required(ErrorMessage = "OGRN is required.")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "OGRN must be 13 characters.")]
    public string Ogrn { get; set; } = string.Empty;

    [Required(ErrorMessage = "KPP is required.")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "KPP must be 9 characters.")]
    public string Kpp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Registration date is required.")]
    public DateOnly RegistrationDate { get; set; }

    [Required(ErrorMessage = "Legal address is required.")]
    public string LegalAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required.")]
    [StringLength(255, ErrorMessage = "Contact email cannot exceed 255 characters.")]
    [EmailAddress(ErrorMessage = "Invalid contact email format.")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone is required.")]
    [StringLength(50, ErrorMessage = "Contact phone cannot exceed 50 characters.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "IndustryFieldId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "IndustryFieldId must be a positive number.")]
    public int IndustryFieldId { get; set; }
}
