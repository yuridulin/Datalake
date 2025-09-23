FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Apps/Collector/Collector.csproj", "Apps/Collector/"]
COPY ["Database/Database.csproj",  "Database/"]
COPY ["Api/PublicApi/PublicApi.csproj", "Api/PublicApi/"]
COPY ["Api/PrivateApi/PrivateApi.csproj", "Api/PrivateApi/"]
RUN dotnet restore "Apps/Collector/Collector.csproj"
COPY . .
WORKDIR "/src/Apps/Collector"
RUN dotnet build "./Collector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Collector.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Datalake.Collector.dll"]