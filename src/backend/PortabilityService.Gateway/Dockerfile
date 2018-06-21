### Build Stage
FROM microsoft/dotnet:2.1-sdk-alpine AS build

COPY . .

# Restore packages
RUN dotnet restore src/backend/PortabilityService.Gateway/PortabilityService.Gateway.csproj

# Build
RUN dotnet build src/backend/PortabilityService.Gateway/PortabilityService.Gateway.csproj -c Release -o /app

### Publish
FROM build AS publish
RUN dotnet publish src/backend/PortabilityService.Gateway/PortabilityService.Gateway.csproj -c Release -o /app

### Run Stage
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.Gateway.dll"]
