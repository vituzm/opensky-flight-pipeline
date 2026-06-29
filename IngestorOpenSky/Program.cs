using IngestorOpenSky;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Services;
using IngestorOpenSky.Models;
using IngestorOpenSky.Models.Config;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IOpenSkyClient, OpenSkyClient>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IOpenSkyDataMapper, OpenSkyDataMapper>();
builder.Services.AddSingleton<IEventFailureRepository, EventFailureRepository>();
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<OpenSkyOptions>(builder.Configuration.GetSection("OpenSky"));

var host = builder.Build();

host.Run();
