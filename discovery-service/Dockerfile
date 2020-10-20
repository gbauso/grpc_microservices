FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["contract/*", "contract/"]
COPY ["src/DiscoveryService/DiscoveryService.csproj", "src/DiscoveryService/"]

RUN dotnet restore "src/DiscoveryService/DiscoveryService.csproj"
COPY . .
WORKDIR "/src/src/DiscoveryService"
RUN dotnet build "DiscoveryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscoveryService.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscoveryService.dll"]