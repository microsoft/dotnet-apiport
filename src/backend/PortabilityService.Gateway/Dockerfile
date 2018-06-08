### Build Stage
FROM microsoft/dotnet:2.1-sdk-alpine AS build
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
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.Gateway.dll"]
