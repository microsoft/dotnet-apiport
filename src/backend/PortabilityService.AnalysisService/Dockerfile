# We cache these documents here because they seldom change.
# Updating the service when the documents change will require rebuilding the image.
FROM microsoft/dotnet:2.1-sdk-alpine AS github
WORKDIR /github
RUN wget https://github.com/Microsoft/dotnet/archive/master.zip -O dotnet.zip && \
    unzip dotnet.zip "dotnet-master/Documentation/compatibility/*.md" -q
RUN wget https://github.com/Microsoft/dotnet-apiport/archive/master.zip -O apiport.zip && \
    unzip apiport.zip "dotnet-apiport-master/docs/RecommendedChanges/*" -q

FROM github AS build
COPY . .
RUN dotnet restore src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj
RUN dotnet build src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish src/backend/PortabilityService.AnalysisService/PortabilityService.AnalysisService.csproj -c Release -o /app

FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=github /github/dotnet-master/Documentation/compatibility/ ./breaking-changes
COPY --from=github /github/dotnet-apiport-master/docs/RecommendedChanges/ ./recommended-changes
COPY --from=publish /app .
ENV BreakingChangesPath=breaking-changes \
    RecommendedChangesPath=recommended-changes
EXPOSE 80
ENTRYPOINT ["dotnet", "PortabilityService.AnalysisService.dll"]
