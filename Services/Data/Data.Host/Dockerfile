FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/DataService/DataService.csproj", "Services/DataService/"]
COPY ["Domains/DataDomain/DataDomain.csproj",  "Domains/DataDomain/"]
COPY ["Api/PublicApi/PublicApi.csproj", "Api/PublicApi/"]
COPY ["Api/PrivateApi/PrivateApi.csproj", "Api/PrivateApi/"]
RUN dotnet restore "Services/DataService/DataService.csproj"
COPY . .
WORKDIR "/src/Services/DataService"
RUN dotnet build "./DataService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./DataService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Datalake.DataService.dll"]