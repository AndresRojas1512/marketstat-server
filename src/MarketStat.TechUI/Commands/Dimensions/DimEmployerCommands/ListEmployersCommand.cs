using MarketStat.Services.Dimensions.DimEmployerService;

namespace MarketStat.TechUI.Commands.Dimensions.DimEmployerCommands;

public class ListEmployersCommand : ICommand
{
    public string Name => "list-employers";
    public string HelpText => "list-employers      – show all employers";
    
    private readonly IDimEmployerService _dimEmployerService;

    public ListEmployersCommand(IDimEmployerService dimEmployerService)
    {
        _dimEmployerService = dimEmployerService;
    }
    public async Task ExecuteAsync(string[] args)
    {
        var all = await _dimEmployerService.GetAllEmployersAsync();
        foreach (var e in all)
            Console.WriteLine($"{e.EmployerId}: {e.EmployerName} Public={e.IsPublic}");
    }
}
