using IngestorOpenSky;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Services;
using IngestorOpenSky.Models;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IOpenSkyClient, OpenSkyClient>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IOpenSkyDataMapper, OpenSkyDataMapper>();
builder.Services.AddSingleton<IEventFailureRepository, EventFailureRepository>();

var host = builder.Build();

host.Run();
