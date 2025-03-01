using Contracts;
using FastEndpoints.Messaging.Remote.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Test;

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
                        s.RegisterTestRemote(_publisher.Server);                           //connect subscriber to publisher
                        s.RegisterTestEventHandler<SomethingHappened, TestEventHandler>(); //using a fake handler to assert receipt of events
                    });
            }).Server;
    }

#region disposable

    bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                PublisherClient.Dispose();
                _subscriber.Dispose();
                _publisher.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

#endregion
}