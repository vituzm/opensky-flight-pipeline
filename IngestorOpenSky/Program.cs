using IngestorOpenSky;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Services;
using IngestorOpenSky.Models;
using IngestorOpenSky.Models.Config;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Options;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient();

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("KafkaProducer"));
builder.Services.Configure<OpenSkyOptions>(builder.Configuration.GetSection("OpenSky"));
builder.Services.Configure<EventFailureRepositoryOptions>(builder.Configuration.GetSection("EventFailureRepository"));
builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("SchemaRegistry"));

builder.Services.AddSingleton<IOpenSkyClient, OpenSkyClient>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IOpenSkyDataMapper, OpenSkyDataMapper>();
builder.Services.AddSingleton<IEventFailureRepository, EventFailureRepository>();

builder.Services.AddSingleton<IProducer<string, byte[]>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>().Value;
    return new ProducerBuilder<string, byte[]>(config)
            .SetLogHandler((_, logMessage) => 
            {
                
            })
            .Build();
});

builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>().Value;
    return new CachedSchemaRegistryClient(config);
});

var host = builder.Build();

host.Run();
