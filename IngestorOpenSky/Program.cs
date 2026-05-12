using IngestorOpenSky;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IOpenSkyClient, OpenSkyClient>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

var host = builder.Build();

host.Run();
