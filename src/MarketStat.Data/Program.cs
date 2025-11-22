using MarketStat.Data.Consumers.Facts;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var connString = hostContext.Configuration.GetConnectionString("MarketStat");
        services.AddDbContext<MarketStatDbContext>(opts =>
            opts.UseNpgsql(connString, o => o.EnableRetryOnFailure())
                .UseSnakeCaseNamingConvention());
        services.AddScoped<IFactSalaryRepository, FactSalaryRepository>();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<FactSalaryDataConsumer>();
            x.AddConsumer<GetFactSalaryConsumer>();
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