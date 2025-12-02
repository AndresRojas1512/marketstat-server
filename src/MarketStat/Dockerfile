FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

#COPY src/MarketStat.sln .
#
#COPY src/MarketStat/MarketStat.csproj MarketStat/
#COPY src/MarketStat.Common/MarketStat.Common.Converter/MarketStat.Common.Converter.csproj MarketStat.Common/MarketStat.Common.Converter/
#COPY src/MarketStat.Common/MarketStat.Common.Core/MarketStat.Common.Core.csproj MarketStat.Common/MarketStat.Common.Core/
#COPY src/MarketStat.Common/MarketStat.Common.Dto/MarketStat.Common.Dto.csproj MarketStat.Common/MarketStat.Common.Dto/
#COPY src/MarketStat.Common/MarketStat.Common.Enums/MarketStat.Common.Enums.csproj MarketStat.Common/MarketStat.Common.Enums/
#COPY src/MarketStat.Common/MarketStat.Common.Exceptions/MarketStat.Common.Exceptions.csproj MarketStat.Common/MarketStat.Common.Exceptions/
#COPY src/MarketStat.Database/MarketStat.Database.Context/MarketStat.Database.Context.csproj MarketStat.Database/MarketStat.Database.Context/
#COPY src/MarketStat.Database/MarketStat.Database.Core/MarketStat.Database.Core.csproj MarketStat.Database/MarketStat.Database.Core/
#COPY src/MarketStat.Database/MarketStat.Database.Models/MarketStat.Database.Models.csproj MarketStat.Database/MarketStat.Database.Models/
#COPY src/MarketStat.Database/MarketStat.Database.Repositories/MarketStat.Database.Repositories.csproj MarketStat.Database/MarketStat.Database.Repositories/
#COPY src/MarketStat.Services/MarketStat.Services.Auth/MarketStat.Services.Auth.AuthService/MarketStat.Services.Auth.AuthService.csproj MarketStat.Services/MarketStat.Services.Auth/MarketStat.Services.Auth.AuthService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimDateService/MarketStat.Services.Dimensions.DimDateService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimDateService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEducationService/MarketStat.Services.Dimensions.DimEducationService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEducationService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployeeService/MarketStat.Services.Dimensions.DimEmployeeService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployeeService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployerService/MarketStat.Services.Dimensions.DimEmployerService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimEmployerService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimIndustryFieldService/MarketStat.Services.Dimensions.DimIndustryFieldService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimIndustryFieldService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimJobService/MarketStat.Services.Dimensions.DimJobService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimJobService/
#COPY src/MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimLocationService/MarketStat.Services.Dimensions.DimLocationService.csproj MarketStat.Services/MarketStat.Services.Dimensions/MarketStat.Services.Dimensions.DimLocationService/
#COPY src/MarketStat.Services/MarketStat.Services.Facts/MarketStat.Services.Facts.FactSalaryService/MarketStat.Services.Facts.FactSalaryService.csproj MarketStat.Services/MarketStat.Services.Facts/MarketStat.Services.Facts.FactSalaryService/
#
#COPY src/MarketStat.Tests/MarketStat.Services.Tests/MarketStat.Services.Tests.csproj MarketStat.Tests/MarketStat.Services.Tests/
#COPY src/MarketStat.Tests/MarketStat.Repository.Tests/MarketStat.Repository.Tests.csproj MarketStat.Tests/MarketStat.Repository.Tests/
#COPY src/MarketStat.Tests/MarketStat.Tests.TestData/MarketStat.Tests.TestData.csproj MarketStat.Tests/MarketStat.Tests.TestData/
#COPY src/MarketStat.Tests/MarketStat.Integration.Tests/MarketStat.Integration.Tests.csproj MarketStat.Tests/MarketStat.Integration.Tests/
#COPY src/MarketStat.Tests/MarketStat.Tests.E2E/MarketStat.Tests.E2E.csproj MarketStat.Tests/MarketStat.Tests.E2E/

COPY src/. .

RUN dotnet restore MarketStat.sln

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