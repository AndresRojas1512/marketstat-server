using System.Text;
using MarketStat.Contracts.Auth;
using MarketStat.Contracts.Dimensions.DimDate;
using MarketStat.Contracts.Facts;
using MarketStat.Contracts.Facts.Analytics;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    
    x.AddRequestClient<ILoginRequest>();
    
    x.AddRequestClient<IGetDimDateRequest>();
    x.AddRequestClient<IGetAllDimDatesRequest>();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
