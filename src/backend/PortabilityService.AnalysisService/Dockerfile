### Build Stage
FROM microsoft/aspnetcore-build:2.0.7-2.1.105 AS build
WORKDIR /src

# Restore packages in one build layer
COPY NuGet.config .
COPY src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj backend/PortabilityService.AnalysisService/
RUN dotnet restore backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj

# Build the application in the next
COPY ./src .
WORKDIR /src/backend/PortabilityService.AnalysisService
RUN dotnet build PortabilityService.AnalysisService.csproj -c Release -o /app

### Publish Stage
FROM build AS publish
RUN dotnet publish PortabilityService.AnalysisService.csproj -c Release -o /app

### Run Stage
FROM microsoft/aspnetcore:2.0.7 AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.AnalysisService.dll"]