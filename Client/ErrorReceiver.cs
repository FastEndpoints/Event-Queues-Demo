using FastEndpoints;
using MongoDB.Entities;

namespace SubscriberClient;

internal sealed class ErrorReceiver : SubscriberExceptionReceiver
{
    private readonly DbContext db;

    public ErrorReceiver(DbContext dbContext, ILogger<ErrorReceiver> logger)
    {
        db = dbContext;
        logger.LogInformation("Subscriber Error Receiver Initialized!");
    }

    public override async Task OnMarkEventAsCompleteError<TEvent>(IEventStorageRecord record, int attemptCount, Exception exception, CancellationToken ct)
    {
        var r = (EventRecord)record;
        r.IsComplete = true;
        await r.SaveAsync();
    }
}