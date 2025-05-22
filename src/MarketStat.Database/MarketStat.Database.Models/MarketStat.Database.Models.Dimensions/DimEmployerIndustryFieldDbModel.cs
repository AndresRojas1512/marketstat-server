using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_employer_industry_field")]
public class DimEmployerIndustryFieldDbModel
{
    [Column("employer_id")]
    public int EmployerId { get; set; }
    
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }
    
    [ForeignKey(nameof(EmployerId))]
    public virtual DimEmployerDbModel? Employer { get; set; }
    
    [ForeignKey(nameof(IndustryFieldId))]
    public virtual DimIndustryFieldDbModel? IndustryField { get; set; }
    
    public DimEmployerIndustryFieldDbModel() { }

    public DimEmployerIndustryFieldDbModel(int employerId, int industryFieldId)
    {
        EmployerId = employerId;
        IndustryFieldId = industryFieldId;
    }
}