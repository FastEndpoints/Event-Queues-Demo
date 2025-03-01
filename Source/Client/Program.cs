using Contracts;
using FastEndpoints;
using SubscriberClient;

var bld = WebApplication.CreateBuilder();
bld.Services
   .AddSingleton(new DbContext("SubscriberEventStore", "localhost"))
   .AddEventSubscriberStorageProvider<EventRecord, SubscriberStorageProvider>();

//bld.Services.AddSubscriberExceptionReceiver<ErrorReceiver>();

var app = bld.Build();

app.MapRemote(
    "http://localhost:6000",
    c =>
    {
        c.Subscribe<SomethingHappened, WhenSomethingHappens>();
    });

app.Run();