using System.Diagnostics.CodeAnalysis;

namespace MarketStat.Integration.Tests;

[CollectionDefinition("Integration")]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Standard naming convention for xUnit Collection Definitions.")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
