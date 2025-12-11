using MarketStat.Contracts.Auth;
using MarketStat.Contracts.Dimensions.DimEducation;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MarketStat.Domain.Consumers.Auth;
using MarketStat.Domain.Consumers.Dimensions;
using MarketStat.Domain.Consumers.Facts;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "MarketStat.Domain")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        uri: "http://loki:3100", 
        labels: new[] { new LokiLabel { Key = "service", Value = "domain" } }
    )
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MarketStat Domain Service...");

    IHost host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService("MarketStat.Domain"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddMassTransitInstrumentation()
                        .AddOtlpExporter(opts =>
                        {
                            var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                            if (!string.IsNullOrEmpty(endpoint))
                            {
                                opts.Endpoint = new Uri(endpoint);
                            }
                        });
                });

            services.AddMassTransit(x =>
            {
                x.AddConsumer<FactSalaryDomainConsumer>();
                x.AddConsumer<AuthDomainConsumer>();
                x.AddConsumer<DimDateDomainConsumer>();
                x.AddConsumer<DimEducationDomainConsumer>();
                x.AddConsumer<DimEmployeeDomainConsumer>();
                x.AddConsumer<DimEmployerDomainConsumer>();
                x.AddConsumer<DimIndustryFieldDomainConsumer>();
                x.AddConsumer<DimJobDomainConsumer>();
                x.AddConsumer<DimLocationDomainConsumer>();

                x.AddRequestClient<IGetDimIndustryFieldRequest>();
                x.AddRequestClient<IGetDimEducationRequest>();

                x.AddRequestClient<IGetUserAuthDetailsRequest>();
                x.AddRequestClient<IPersistUserCommand>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Domain Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
