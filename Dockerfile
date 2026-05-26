FROM node:22-bookworm-slim AS client-build
WORKDIR /src/reactapp1.client

COPY reactapp1.client/package*.json ./
RUN npm ci

COPY reactapp1.client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server-build
WORKDIR /src

RUN apt-get update \
	&& apt-get install -y --no-install-recommends ca-certificates curl gnupg \
	&& curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
	&& apt-get install -y --no-install-recommends nodejs \
	&& rm -rf /var/lib/apt/lists/*

COPY ReactApp1.Server/ReactApp1.Server.csproj ReactApp1.Server/
COPY reactapp1.client/reactapp1.client.esproj reactapp1.client/
RUN dotnet restore ReactApp1.Server/ReactApp1.Server.csproj

COPY . .
RUN rm -rf ReactApp1.Server/wwwroot/*
COPY --from=client-build /src/reactapp1.client/dist/ ReactApp1.Server/wwwroot/
RUN dotnet publish ReactApp1.Server/ReactApp1.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV PORT=8080
EXPOSE 8080

COPY --from=server-build /app/publish/ .

ENTRYPOINT ["dotnet", "ReactApp1.Server.dll"]