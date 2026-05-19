FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ReactApp1.Server/ReactApp1.Server.csproj", "ReactApp1.Server/"]
RUN dotnet restore "ReactApp1.Server/ReactApp1.Server.csproj"

COPY . .
RUN dotnet build "ReactApp1.Server/ReactApp1.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReactApp1.Server/ReactApp1.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ReactApp1.Server.dll"]
