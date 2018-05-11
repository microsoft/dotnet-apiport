### Build Stage
FROM microsoft/aspnetcore-build:2.0.7-2.1.105 AS build
WORKDIR /src

COPY NuGet.config .
COPY ./src .
WORKDIR /src/backend/PortabilityService.AnalysisEngine
RUN dotnet publish PortabilityService.AnalysisEngine.csproj -c Release -o /app


### Run Stage
FROM microsoft/aspnetcore:2.0.7 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.AnalysisEngine.dll"]