# --- 1. Рантайм (финальный базовый образ) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root

# Устанавливаем curl для healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
USER app
EXPOSE 8080 8081

# --- 2. Сборка фронтенда на Node.js ---
FROM node:20-slim AS client-build
WORKDIR /src/Clients/datalake

# первый слой: package.json ⇒ npm ci (кэшируется)
COPY Clients/datalake/package.json      ./package.json
COPY Clients/datalake/package-lock.json ./package-lock.json
RUN npm ci

# копируем весь фронтенд и билдим
COPY Clients/datalake ./
RUN npm run build

# --- 3. Сборка .NET SDK ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 3.1. Кэшируем restore: копируем все csproj, привязанные к Server
COPY ["Gateway/GatewayService.csproj", "Gateway/"]
COPY ["Api/PublicApi/PublicApi.csproj", "Api/PublicApi/"]
COPY ["Api/PrivateApi/PrivateApi.csproj", "Api/PrivateApi/"]

# 3.2. Делаем restore — в это время подтягиваются пакеты и ссылки на Database/PublicApi
RUN dotnet restore "Gateway/GatewayService.csproj"

# 3.3. Копируем остальные исходники
COPY ["Gateway/", "Gateway/"]
COPY ["PublicApi/", "PublicApi/"]
COPY ["PrivateApi/", "PrivateApi/"]

# 3.4. Интегрируем готовый фронтенд в wwwroot
COPY --from=client-build /src/Clients/datalake/dist/ Gateway/wwwroot/

# 3.5. Собираем Release-билд
WORKDIR /src/Gateway
RUN dotnet build "GatewayService.csproj" -c Release -o /app/build

# Версия
ARG APP_VERSION=dev
ENV APP_VERSION=${APP_VERSION}

# --- 4. Публикация (publish без apphost) ---
FROM build AS publish
ARG APP_VERSION=dev
WORKDIR /src/Gateway
RUN dotnet publish "GatewayService.csproj" \
	-c Release \
	-p:Version=${APP_VERSION} \
	-p:SourceRevisionId=${APP_VERSION} \
	-p:UseAppHost=false \
	-o /app/publish

# --- 5. Финальный образ ---
FROM base AS final
WORKDIR /app
ARG APP_VERSION=dev
ENV APP_VERSION=${APP_VERSION}

# Копируем всё из publish
COPY --from=publish /app/publish .

# Делаем папку доступной
USER root
RUN chmod -R 777 /app

# Запуск
ENTRYPOINT ["dotnet", "Datalake.GatewayService.dll"]