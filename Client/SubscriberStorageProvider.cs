using FastEndpoints;
using MongoDB.Entities;

namespace SubscriberClient;

public class SubscriberStorageProvider : IEventSubscriberStorageProvider
{
    private readonly DbContext db;

    public SubscriberStorageProvider(DbContext db)
    {
        this.db = db;
    }

    public async ValueTask StoreEventAsync(IEventStorageRecord e, CancellationToken ct)
    {
        e.ExpireOn = DateTime.UtcNow.AddHours(24); //override default expiry time
        await db.SaveAsync((EventRecord)e, ct);
    }

    public async ValueTask<IEventStorageRecord?> GetNextEventAsync(string subscriberID, CancellationToken ct)
    {
        return await db
            .Find<EventRecord>()
            .Match(e => e.SubscriberID == subscriberID && !e.IsComplete && DateTime.UtcNow <= e.ExpireOn)
            .Sort(e => e.ID, Order.Ascending)
            .ExecuteFirstAsync(ct);
    }

    public async ValueTask MarkEventAsCompleteAsync(IEventStorageRecord e, CancellationToken ct)
    {
        await db
            .Update<EventRecord>()
            .MatchID(((EventRecord)e).ID)
            .Modify(e => e.IsComplete, true)
            .ExecuteAsync(ct);
    }

    public async ValueTask PurgeStaleRecordsAsync()
    {
        await db.DeleteAsync<EventRecord>(e => e.IsComplete || (!e.IsComplete && DateTime.UtcNow >= e.ExpireOn));
    }
}