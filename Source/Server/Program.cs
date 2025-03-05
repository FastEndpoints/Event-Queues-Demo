using Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PublisherServer;

var bld = WebApplication.CreateBuilder(args);

bld.WebHost.ConfigureKestrel(
    o =>
    {
        o.ListenLocalhost(6000, l => l.Protocols = HttpProtocols.Http2);         // for GRPC
        o.ListenLocalhost(5001, l => l.Protocols = HttpProtocols.Http1AndHttp2); // for REST
    });

bld.AddHandlerServer().Services
   .AddSingleton(new DbContext("PublisherEventStore", "localhost"));

var app = bld.Build();
app.MapHandlers<EventRecord, HubStorageProvider>(
    h =>
    {
        h.RegisterEventHub<SomethingHappened>();
    });

app.MapGet(
    "/event/{name}",
    async (string name) =>
    {
        for (var i = 1; i <= 10; i++)
        {
            new SomethingHappened
                {
                    Id = i,
                    Description = name
                }
                .Broadcast();

            await Task.Delay(100);
        }

        return Results.Ok("events published!");
    });

app.Run();