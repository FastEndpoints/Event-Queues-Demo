using FastEndpoints;
using MongoDB.Entities;

namespace SubscriberClient;

sealed class ErrorReceiver : SubscriberExceptionReceiver
{
    readonly DbContext db;

    public ErrorReceiver(DbContext dbContext, ILogger<ErrorReceiver> logger)
    {
        db = dbContext;
        logger.LogInformation("Subscriber Error Receiver Initialized!");
    }

    public override async Task OnMarkEventAsCompleteError<TEvent>(IEventStorageRecord record, int attemptCount, Exception exception, CancellationToken ct)
    {
        var r = (EventRecord)record;
        r.IsComplete = true;
        await db.SaveAsync(r, ct);
    }
}