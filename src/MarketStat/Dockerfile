FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/MarketStat.sln .
COPY src/MarketStat/MarketStat.csproj src/MarketStat/

COPY src/MarketStat.Common/MarketStat.Common.Converter/MarketStat.Common.Converter.csproj src/MarketStat.Common/MarketStat.Common.Converter/
COPY src/MarketStat.Common/MarketStat.Common.Core/MarketStat.Common.Core.csproj src/MarketStat.Common/MarketStat.Common.Core/
COPY src/MarketStat.Common/MarketStat.Common.Dto/MarketStat.Common.Dto.csproj src/MarketStat.Common/MarketStat.Common.Dto/
COPY src/MarketStat.Common/MarketStat.Common.Enums/MarketStat.Common.Enums.csproj src/MarketStat.Common/MarketStat.Common.Enums/
COPY src/MarketStat.Common/MarketStat.Common.Exceptions/MarketStat.Common.Exceptions.csproj src/MarketStat.Common/MarketStat.Common.Exceptions/

COPY src/MarketStat.Database/MarketStat.Database.Context/MarketStat.Database.Context.csproj src/MarketStat.Database/MarketStat.Database.Context/
COPY src/MarketStat.Database/MarketStat.Database.Core/MarketStat.Database.Core.csproj src/MarketStat.Database/MarketStat.Database.Core/
COPY src/MarketStat.Database/MarketStat.Database.Models/MarketStat.Database.Models.csproj src/MarketStat.Database/MarketStat.Database.Models/
COPY src/MarketStat.Database/MarketStat.Database.Repositories/MarketStat.Database.Repositories.csproj src/MarketStat.Database/MarketStat.Database.Repositories/

COPY src/MarketStat.Services/MarketStat.Services.Auth/MarketStat.Services.Auth.AuthService/MarketStat.Services.Auth.AuthService.csproj src/MarketStat.Services/MarketStat.Services.Auth/MarketStat.Services.Auth.AuthService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimDateService/MarketStat.Services.Dimensions.DimDateService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimDateService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEducationService/MarketStat.Services.Dimensions.DimEducationService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEducationService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployeeService/MarketStat.Services.Dimensions.DimEmployeeService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployeeService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployerService/MarketStat.Services.Dimensions.DimEmployerService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployerService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimIndustryFieldService/MarketStat.Services.Dimensions.DimIndustryFieldService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimIndustryFieldService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimJobService/MarketStat.Services.Dimensions.DimJobService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimJobService/
COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimLocationService/MarketStat.Services.Dimensions.DimLocationService.csproj src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimLocationService/
COPY src/MarketStat.Services/MarketStat.Services.Facts/MarketStat.Services.Facts.FactSalaryService/MarketStat.Services.Facts.FactSalaryService.csproj src/MarketStat.Services/MarketStat.Services.Facts/MarketStat.Services.Facts.FactSalaryService/

RUN dotnet restore src/MarketStat.sln

COPY src/ .

WORKDIR "/src/MarketStat"
RUN dotnet build "MarketStat.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketStat.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MarketStat.dll"]