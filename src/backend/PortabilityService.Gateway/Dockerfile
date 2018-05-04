### Build Stage
FROM microsoft/aspnetcore-build:2.0.7-2.1.105 AS build
WORKDIR /src

# Restore packages in one build layer
COPY NuGet.config .
COPY src/backend/PortabilityService.Gateway/PortabilityService.Gateway.csproj backend/PortabilityService.Gateway/
RUN dotnet restore backend/PortabilityService.Gateway/PortabilityService.Gateway.csproj

# Build the application in the next
COPY ./src .
WORKDIR /src/backend/PortabilityService.Gateway
RUN dotnet build PortabilityService.Gateway.csproj -c Release -o /app


### Publish Stage
FROM build AS publish
RUN dotnet publish PortabilityService.Gateway.csproj -c Release -o /app


### Run Stage
FROM microsoft/aspnetcore:2.0.7 AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.Gateway.dll"]
