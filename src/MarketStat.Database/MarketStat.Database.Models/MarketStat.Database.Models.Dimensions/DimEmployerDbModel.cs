using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_employer")]
public class DimEmployerDbModel
{
    [Key]
    [Column("employer_id")]
    public int EmployerId { get; set; }

    [Required]
    [Column("employer_name")]
    [StringLength(255)]
    public string EmployerName { get; set; } = string.Empty;
    
    [Required]
    [Column("inn")]
    [StringLength(12)]
    public string Inn { get; set; } = string.Empty;
    
    [Required]
    [Column("ogrn")]
    [StringLength(13)]
    public string Ogrn { get; set; } = string.Empty;
    
    [Required]
    [Column("kpp")]
    [StringLength(9)]
    public string Kpp { get; set; } = string.Empty;
    
    [Required]
    [Column("registration_date", TypeName = "date")]
    public DateOnly RegistrationDate { get; set; }
    
    [Required]
    [Column("legal_address", TypeName = "text")]
    public string LegalAddress { get; set; } = string.Empty;
    
    [Required]
    [Column("contact_email")]
    [StringLength(255)]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Required]
    [Column("contact_phone")]
    [StringLength(50)]
    public string ContactPhone { get; set; } = string.Empty;
    
    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }
    
    [ForeignKey(nameof(IndustryFieldId))]
    public virtual DimIndustryFieldDbModel DimIndustryField { get; set; }
    
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; }

    public DimEmployerDbModel()
    {
        FactSalaries = new List<FactSalaryDbModel>();
    }
}