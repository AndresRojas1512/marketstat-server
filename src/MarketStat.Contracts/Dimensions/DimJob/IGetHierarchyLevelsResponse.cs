namespace MarketStat.Contracts.Dimensions.DimJob;

public interface IGetHierarchyLevelsResponse
{
    List<string> Levels { get; }
}