using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_employer_industry_field")]
public class DimEmployerIndustryFieldDbModel
{
    [Key, Column("employer_id", Order = 0)]
    public int EmployerId { get; set; }
    
    [Key, Column("industry_field_id", Order = 1)]
    public int IndustryFieldId { get; set; }

    public DimEmployerIndustryFieldDbModel(int employerId, int industryFieldId)
    {
        EmployerId = employerId;
        IndustryFieldId = industryFieldId;
    }
}