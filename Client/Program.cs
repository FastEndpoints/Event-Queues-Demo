using Contracts;
using FastEndpoints;
using SubscriberClient;

var bld = WebApplication.CreateBuilder();
var app = bld.Build();

app.MapRemote("http://localhost:6000", c =>
{
    c.Subscribe<SomethingHappened, WhenSomethingHappens>();
});

app.Run();
