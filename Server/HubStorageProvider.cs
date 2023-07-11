using FastEndpoints;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;

namespace PublisherServer;

public class HubStorageProvider : IEventHubStorageProvider
{
    private readonly DbContext db;

    public HubStorageProvider(DbContext db)
    {
        this.db = db;
    }

    public async ValueTask<IEnumerable<string>> RestoreSubsriberIDsForEventType(string eventType)
    {
        return await db
            .Queryable<EventRecord>()
            .Where(e => e.EventType == eventType && !e.IsComplete && DateTime.UtcNow <= e.ExpireOn)
            .Select(e => e.SubscriberID)
            .Distinct()
            .ToListAsync();
    }

    public async ValueTask StoreEventAsync(IEventStorageRecord e, CancellationToken ct)
    {
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