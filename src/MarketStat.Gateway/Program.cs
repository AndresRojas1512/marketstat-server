using System.Text;
using MarketStat.Contracts.Auth;
using MarketStat.Contracts.Dimensions.DimDate;
using MarketStat.Contracts.Dimensions.DimEducation;
using MarketStat.Contracts.Dimensions.DimEmployee;
using MarketStat.Contracts.Dimensions.DimEmployer;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MarketStat.Contracts.Dimensions.DimJob;
using MarketStat.Contracts.Dimensions.DimLocation;
using MarketStat.Contracts.Facts;
using MarketStat.Contracts.Facts.Analytics;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "MarketStat.Gateway")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        uri: "http://loki:3100",
        labels: new[] { new LokiLabel { Key = "service", Value = "gateway" } }
    )
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        { 
            Title = "MarketStat Gateway API", 
            Version = "v1" 
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        });
    });
    
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService("MarketStat.Gateway"))
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
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

    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("rabbitmq", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
            cfg.ConfigureEndpoints(context);
        });

        x.AddRequestClient<IGetFactSalaryRequest>();
        x.AddRequestClient<IGetFactSalariesByFilterRequest>();
        x.AddRequestClient<IGetFactSalaryDistributionRequest>();
        x.AddRequestClient<IGetFactSalarySummaryRequest>();
        x.AddRequestClient<IGetFactSalaryTimeSeriesRequest>();
        x.AddRequestClient<IGetPublicRolesRequest>();

        x.AddRequestClient<ISubmitRegisterCommand>();
        x.AddRequestClient<ILoginRequest>();

        x.AddRequestClient<IGetDimDateRequest>();
        x.AddRequestClient<IGetAllDimDatesRequest>();

        x.AddRequestClient<IGetDimEducationRequest>();
        x.AddRequestClient<IGetAllDimEducationsRequest>();

        x.AddRequestClient<IGetDimEmployeeRequest>();
        x.AddRequestClient<IGetAllDimEmployeesRequest>();

        x.AddRequestClient<IGetDimEmployerRequest>();
        x.AddRequestClient<IGetAllDimEmployersRequest>();

        x.AddRequestClient<IGetDimIndustryFieldRequest>();
        x.AddRequestClient<IGetAllDimIndustryFieldsRequest>();

        x.AddRequestClient<IGetDimJobRequest>();
        x.AddRequestClient<IGetAllDimJobsRequest>();
        x.AddRequestClient<IGetStandardJobRolesRequest>();
        x.AddRequestClient<IGetHierarchyLevelsRequest>();

        x.AddRequestClient<IGetDimLocationRequest>();
        x.AddRequestClient<IGetAllDimLocationsRequest>();
        x.AddRequestClient<IGetDistrictsRequest>();
        x.AddRequestClient<IGetOblastsRequest>();
        x.AddRequestClient<IGetCitiesRequest>();
    });

    var jwtKey = builder.Configuration["JwtSettings:Key"];
    if (!string.IsNullOrEmpty(jwtKey))
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
    }

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => 
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketStat API v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
