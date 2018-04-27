### Build Stage
FROM microsoft/aspnetcore-build:2.0.7-2.1.105 AS build
WORKDIR /src

# Restore packages in one build layer
COPY NuGet.config .
COPY src/backend/PortabilityService.ConfigurationService/PortabilityService.ConfigurationService.csproj backend/PortabilityService.ConfigurationService/
RUN dotnet restore backend/PortabilityService.ConfigurationService/PortabilityService.ConfigurationService.csproj

# Build the application in the next
COPY ./src .
WORKDIR /src/backend/PortabilityService.ConfigurationService
RUN dotnet build PortabilityService.ConfigurationService.csproj -c Release -o /app


### Publish Stage
FROM build AS publish
RUN dotnet publish PortabilityService.ConfigurationService.csproj -c Release -o /app


### Run Stage
FROM microsoft/aspnetcore:2.0.7 AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.ConfigurationService.dll"]
