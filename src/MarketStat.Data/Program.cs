using MarketStat.Data;
using MarketStat.Data.Consumers;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        var connectionString = configuration.GetConnectionString("MarketStat");

        services.AddDbContext<MarketStatDbContext>(options =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })
                .UseSnakeCaseNamingConvention();
        });
        services.AddScoped<IFactSalaryRepository, FactSalaryRepository>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<FactSalaryDataConsumer>();
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