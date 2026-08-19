FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lazerboard.ScoreFetcher/Lazerboard.ScoreFetcher.csproj", "Lazerboard.ScoreFetcher/"]
COPY ["Lazerboard.Data/Lazerboard.Data.csproj", "Lazerboard.Data/"]
RUN dotnet restore "Lazerboard.ScoreFetcher/Lazerboard.ScoreFetcher.csproj"
COPY . .
WORKDIR "/src/Lazerboard.ScoreFetcher"
RUN dotnet build "./Lazerboard.ScoreFetcher.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lazerboard.ScoreFetcher.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lazerboard.ScoreFetcher.dll"]

