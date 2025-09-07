# --- 1. Рантайм (финальный базовый образ) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# Устанавливаем curl для healthcheck
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
USER app
EXPOSE 8080 8081

# --- 2. Сборка фронтенда на Node.js ---
FROM node:20-slim AS client-build
WORKDIR /src/Apps/Client/datalake

# первый слой: package.json ⇒ npm ci (кэшируется)
COPY Apps/Client/datalake/package.json     ./package.json
COPY Apps/Client/datalake/package-lock.json ./package-lock.json
RUN npm ci

# копируем весь фронтенд и билдим
COPY Apps/Client/datalake ./
RUN npm run build

# --- 3. Сборка .NET SDK ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 3.1. Кэшируем restore: копируем все csproj, привязанные к Server
COPY ["Apps/Server/Server.csproj", "Apps/Server/"]
COPY ["Database/Database.csproj",  "Database/"]
COPY ["Api/PublicApi/PublicApi.csproj", "Api/PublicApi/"]
COPY ["Api/PrivateApi/PrivateApi.csproj", "Api/PrivateApi/"]

# 3.2. Делаем restore — в это время подтягиваются пакеты и ссылки на Database/PublicApi
RUN dotnet restore "Apps/Server/Server.csproj"

# 3.3. Копируем остальные исходники
COPY ["Apps/Server/", "Apps/Server/"]
COPY ["Database/", "Database/"]
COPY ["PublicApi/", "PublicApi/"]
COPY ["PrivateApi/", "PrivateApi/"]

# 3.4. Интегрируем готовый фронтенд в wwwroot
COPY --from=client-build /src/Apps/Client/datalake/dist/ Apps/Server/wwwroot/

# 3.5. Собираем Release-билд
WORKDIR /src/Apps/Server
RUN dotnet build "Server.csproj" -c Release -o /app/build

# Версия
ARG APP_VERSION=dev
ENV APP_VERSION=${APP_VERSION}

# --- 4. Публикация (publish без apphost) ---
FROM build AS publish
ARG APP_VERSION=dev
WORKDIR /src/Apps/Server
RUN dotnet publish "Server.csproj" \
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

ENTRYPOINT ["dotnet", "Datalake.Server.dll"]