using MarketStat.Contracts.Auth;
using MarketStat.Contracts.Dimensions.DimEducation;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MarketStat.Domain.Consumers.Auth;
using MarketStat.Domain.Consumers.Dimensions;
using MarketStat.Domain.Consumers.Facts;
using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
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
