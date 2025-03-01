using FastEndpoints;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;

namespace PublisherServer;

public class HubStorageProvider : IEventHubStorageProvider<EventRecord>
{
    readonly DbContext db;

    public HubStorageProvider(DbContext db)
    {
        this.db = db;
    }

    public async ValueTask<IEnumerable<string>> RestoreSubscriberIDsForEventTypeAsync(SubscriberIDRestorationParams<EventRecord> p)
        => await db.Queryable<EventRecord>()
                   .Where(p.Match)
                   .Select(p.Projection)
                   .Distinct()
                   .ToListAsync(p.CancellationToken);

    public async ValueTask StoreEventsAsync(IEnumerable<EventRecord> r, CancellationToken ct)
        => await db.SaveAsync(r, ct); //this is a batch insert in mongo

    public async ValueTask<IEnumerable<EventRecord>> GetNextBatchAsync(PendingRecordSearchParams<EventRecord> p)
        => await db.Find<EventRecord>()
                   .Match(p.Match)
                   .Sort(e => e.ID, Order.Ascending)
                   .Limit(p.Limit)
                   .ExecuteAsync(p.CancellationToken);

    public async ValueTask MarkEventAsCompleteAsync(EventRecord r, CancellationToken ct)
        => await db.Update<EventRecord>()
                   .MatchID(r.ID)
                   .Modify(e => e.IsComplete, true)
                   .ExecuteAsync(ct);

    public async ValueTask PurgeStaleRecordsAsync(StaleRecordSearchParams<EventRecord> p)
        => await db.DeleteAsync(p.Match, p.CancellationToken);
}