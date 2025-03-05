namespace Test.Publisher;

public class PublisherTests(PublisherAppFixture app) : IClassFixture<PublisherAppFixture>
{
    [Theory,
     InlineData("A"),
     InlineData("B"),
     InlineData("C")]
    public async Task Endpoint_Broadcasts_The_Event(string data)
    {
        var res = await app.PublisherClient.GetStringAsync($"/event/{data}", TestContext.Current.CancellationToken);
        Assert.Equal("\"events published!\"", res);

        var receiver = app.Services.GetTestEventReceiver<SomethingHappened>();
        var received = await receiver.WaitForMatchAsync(e => e.Description == data, ct: TestContext.Current.CancellationToken);
        Assert.True(received.Any());
    }
}

public class PublisherAppFixture : IDisposable
{
    public HttpClient PublisherClient { get; set; }
    public IServiceProvider Services { get; set; }

    readonly WebApplicationFactory<PublisherServer.Program> _publisher = new();

    public PublisherAppFixture()
    {
        var app = _publisher.WithWebHostBuilder(
            c =>
            {
                c.ConfigureTestServices(
                    s =>
                    {
                        //s.AddSingleton<FakeDbContext>();
                        s.RegisterTestEventReceivers();
                    });
            });

        PublisherClient = app.CreateClient();
        Services = app.Server.Services;
    }

    public void Dispose()
    {
        PublisherClient.Dispose();
        _publisher.Dispose();
    }
}