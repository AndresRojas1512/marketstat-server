using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Helpers;

public class CounterDocument
{
    [BsonId]
    public string Id { get; set; } = null!;

    [BsonElement("sequence_value")]
    public int SequenceValue { get; set; }
}

public static class MongoSequenceHelper
{
    public static async Task<int> GetNextSequenceValueAsync(IMongoCollection<CounterDocument> countersCollection, string sequenceName)
    {
        var filter = Builders<CounterDocument>.Filter.Eq(c => c.Id, sequenceName);
        var update = Builders<CounterDocument>.Update.Inc(c => c.SequenceValue, 1);
        var options = new FindOneAndUpdateOptions<CounterDocument, CounterDocument>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true
        };

        var counter = await countersCollection.FindOneAndUpdateAsync(filter, update, options);
        return counter.SequenceValue;
    }
}