namespace MarketStat.Database.Repositories;

public class BaseRepository
{
    public BaseRepository()
    {
        RepositoryName = GetType().Name;
    }

    protected string RepositoryName { get; }
}
