using ConsumerOpenSky;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using Confluent.SchemaRegistry;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("KafkaConsumer"));
builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("SchemaRegistry"));

builder.Services.AddSingleton<IConsumer<string, byte[]>>(obj =>
{
    var config = obj.GetRequiredService<IOptions<ConsumerConfig>>().Value;
    return new ConsumerBuilder<string, byte[]>(config).Build();
});

builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>().Value;
    return new CachedSchemaRegistryClient(config);
});

var host = builder.Build();
host.Run();
