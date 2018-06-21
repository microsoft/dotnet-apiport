### Build Stage
FROM microsoft/dotnet:2.1-sdk-alpine AS build

COPY . .

# Restore packages
RUN dotnet restore src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj

# Build
RUN dotnet build src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj -c Release -o /app

### Publish Stage
FROM build AS publish
RUN dotnet publish src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj -c Release -o /app

### Run Stage
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.AnalysisService.dll"]