using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

public class PartialUpdateUserDto
{
    [StringLength(255)]
    [MinLength(1)]
    public string? FullName { get; set; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
}