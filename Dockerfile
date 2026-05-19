# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js for React build
RUN curl -fsSL https://deb.nodesource.com/setup_24.x | bash - && apt-get install -y nodejs

COPY ["ReactApp1.Server/ReactApp1.Server.csproj", "ReactApp1.Server/"]
COPY ["package.json", "package-lock.json", "./"]
RUN dotnet restore "ReactApp1.Server/ReactApp1.Server.csproj"

COPY . .

# Build React client
RUN npm install
RUN npm --prefix reactapp1.client install
RUN npm run build

# Build .NET backend
RUN dotnet build "ReactApp1.Server/ReactApp1.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReactApp1.Server/ReactApp1.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "ReactApp1.Server.dll"]
