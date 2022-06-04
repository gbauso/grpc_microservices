FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["contract/*", "contract/"]
COPY ["src/GrpcGateway/GrpcGateway.csproj", "src/GrpcGateway/"]
COPY ["src/Utils.Grpc/src/Utils.Grpc/Utils.Grpc.csproj", "src/Utils.Grpc/src/Utils.Grpc/"]

RUN dotnet restore "src/GrpcGateway/GrpcGateway.csproj"
COPY . .
RUN dotnet build "src/GrpcGateway/GrpcGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/GrpcGateway/GrpcGateway.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrpcGateway.dll"]