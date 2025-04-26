using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using MarketStat.TechUI.Commands.Dimensions.DimEmployerCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MarketStat.TechUI;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg =>
                cfg.AddJsonFile("appsettings.json", optional: false))
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContextFactory<MarketStatDbContext>(opt =>
                    opt.UseNpgsql(ctx.Configuration.GetConnectionString("default")));

                services.AddScoped<IDimEmployerRepository, DimEmployerRepository>();
                services.AddScoped<IDimEmployerService, DimEmployerService>();

                services.AddSingleton<ConsoleApp>();
                services.AddTransient<ICommand, ListEmployersCommand>();
            })
            .Build();

        await host.Services.GetRequiredService<ConsoleApp>().RunAsync();
    }
}

