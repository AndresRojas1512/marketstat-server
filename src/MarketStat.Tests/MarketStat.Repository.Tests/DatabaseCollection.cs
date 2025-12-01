using System.Diagnostics.CodeAnalysis;

namespace MarketStat.Repository.Tests;

[CollectionDefinition("Database collection")]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Standard naming convention for xUnit Collection Definitions.")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
