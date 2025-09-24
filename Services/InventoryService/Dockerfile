FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/InventoryService/InventoryService.csproj", "Services/InventoryService/"]
COPY ["Domains/InventoryDomain/InventoryDomain.csproj",  "Domains/InventoryDomain/"]
COPY ["Api/PublicApi/PublicApi.csproj", "Api/PublicApi/"]
COPY ["Api/PrivateApi/PrivateApi.csproj", "Api/PrivateApi/"]
RUN dotnet restore "Services/InventoryService/InventoryService.csproj"
COPY . .
WORKDIR "/src/Services/InventoryService"
RUN dotnet build "./InventoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./InventoryService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Datalake.InventoryService.dll"]