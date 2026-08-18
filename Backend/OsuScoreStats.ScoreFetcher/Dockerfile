FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OsuScoreStats.ScoreFetcher/OsuScoreStats.ScoreFetcher.csproj", "OsuScoreStats.ScoreFetcher/"]
COPY ["OsuScoreStats.Data/OsuScoreStats.Data.csproj", "OsuScoreStats.Data/"]
RUN dotnet restore "OsuScoreStats.ScoreFetcher/OsuScoreStats.ScoreFetcher.csproj"
COPY . .
WORKDIR "/src/OsuScoreStats.ScoreFetcher"
RUN dotnet build "./OsuScoreStats.ScoreFetcher.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OsuScoreStats.ScoreFetcher.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OsuScoreStats.ScoreFetcher.dll"]

