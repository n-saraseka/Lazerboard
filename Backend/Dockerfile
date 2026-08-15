FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OsuScoreStats/OsuScoreStats.csproj", "OsuScoreStats/"]
RUN dotnet restore "OsuScoreStats/OsuScoreStats.csproj"
COPY . .
WORKDIR "/src/OsuScoreStats"
RUN dotnet build "./OsuScoreStats.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OsuScoreStats.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OsuScoreStats.dll"]
