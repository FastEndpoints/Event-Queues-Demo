using System.Collections.Concurrent;

namespace Test.E2E;

public class E2EEventTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    readonly HttpClient _publisherClient = fixture.PublisherClient;

    [Fact]
    public async Task Published_Events_Are_Received_By_Subscriber()
    {
        var res = await _publisherClient.GetStringAsync("/event/AAAA", TestContext.Current.CancellationToken);

        Assert.Equal("\"events published!\"", res);
        Assert.True(await TestEventHandler.IsTestPassed());
    }
}

sealed class TestEventHandler : IEventHandler<SomethingHappened>
{
    static readonly ConcurrentBag<SomethingHappened> _received = [];

    public Task HandleAsync(SomethingHappened e, CancellationToken ct)
    {
        if (e.Description == "AAAA")
            _received.Add(e);

        return Task.CompletedTask;
    }

    public static async Task<bool> IsTestPassed()
    {
        var start = DateTime.Now;

        while (_received.Count < 10 && DateTime.Now.Subtract(start).TotalSeconds <= 5)
            await Task.Delay(100);

        var ids = !_received.Select(x => x.Id).Except(Enumerable.Range(1, 10)).Any();
        var desc = _received.All(e => e.Description == "AAAA");

        return _received.Count > 0 && ids && desc;
    }
}

public class TestFixture : IDisposable
{
    public HttpClient PublisherClient { get; set; }

    readonly WebApplicationFactory<SubscriberClient.Program> _subscriber = new();
    readonly WebApplicationFactory<PublisherServer.Program> _publisher = new();

    public TestFixture()
    {
        PublisherClient = _publisher.WithWebHostBuilder(
            c =>
            {
                c.ConfigureTestServices(
                    s =>
                    {
                        //s.AddSingleton<FakeDbContext>();
                    });
            }).CreateClient();

        _ = _subscriber.WithWebHostBuilder(
            c =>
            {
                c.ConfigureTestServices(
                    s =>
                    {
                        //s.AddSingleton<FakeDbContext>();
                        s.RegisterTestRemote(_publisher.Server);                           //connect publisher to subscriber
                        s.RegisterTestEventHandler<SomethingHappened, TestEventHandler>(); //using a fake handler to assert receipt of events
                    });
            }).Server;
    }

    public void Dispose()
    {
        PublisherClient.Dispose();
        _subscriber.Dispose();
        _publisher.Dispose();
        GC.SuppressFinalize(this);
    }
}