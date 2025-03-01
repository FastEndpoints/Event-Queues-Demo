using FastEndpoints;

namespace SubscriberClient;

sealed class ErrorReceiver : SubscriberExceptionReceiver
{
    readonly DbContext _db;

    public ErrorReceiver(DbContext dbContext, ILogger<ErrorReceiver> logger)
    {
        _db = dbContext;
        logger.LogInformation("Subscriber Error Receiver Initialized!");
    }

    public override async Task OnMarkEventAsCompleteError<TEvent>(IEventStorageRecord record, int attemptCount, Exception exception, CancellationToken ct)
    {
        var r = (EventRecord)record;
        r.IsComplete = true;
        await _db.SaveAsync(r, ct);
    }
}