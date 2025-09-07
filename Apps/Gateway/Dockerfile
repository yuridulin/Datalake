FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Apps/Gateway/Gateway.csproj", "Apps/Gateway/"]
COPY ["Database/Database.csproj",  "Database/"]
COPY ["Api/PublicApi/PublicApi.csproj", "Api/PublicApi/"]
COPY ["Api/PrivateApi/PrivateApi.csproj", "Api/PrivateApi/"]
RUN dotnet restore "./Apps/Gateway/Gateway.csproj"
COPY . .
WORKDIR "/src/Apps/Gateway"
RUN dotnet build "./Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Datalake.Gateway.dll"]