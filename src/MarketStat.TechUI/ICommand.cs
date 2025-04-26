namespace MarketStat.TechUI;

public interface ICommand
{
    string Name { get; }
    string HelpText { get; }
    Task ExecuteAsync(string[] args);
}