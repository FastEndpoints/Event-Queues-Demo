using FastEndpoints;
using MongoDB.Entities;

namespace SubscriberClient;

public class SubscriberStorageProvider(DbContext db) : IEventSubscriberStorageProvider<EventRecord>
{
    public async ValueTask StoreEventAsync(EventRecord r, CancellationToken ct)
    {
        r.ExpireOn = DateTime.UtcNow.AddHours(24); //override default expiry time
        await db.SaveAsync(r, ct);
    }

    public async ValueTask<IEnumerable<EventRecord>> GetNextBatchAsync(PendingRecordSearchParams<EventRecord> p)
        => await db.Find<EventRecord>()
                   .Match(p.Match)
                   .Sort(e => e.ID, Order.Ascending)
                   .Limit(p.Limit)
                   .ExecuteAsync(p.CancellationToken);

    public async ValueTask MarkEventAsCompleteAsync(EventRecord r, CancellationToken ct)
    {
        //throw new InvalidOperationException("testing exception receiver!");

        await db.Update<EventRecord>()
                .MatchID(r.ID)
                .Modify(e => e.IsComplete, true)
                .ExecuteAsync(ct);
    }

    public async ValueTask PurgeStaleRecordsAsync(StaleRecordSearchParams<EventRecord> p)
        => await db.DeleteAsync(p.Match);
}