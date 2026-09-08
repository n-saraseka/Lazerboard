FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lazerboard.ExternalApis/Lazerboard.ExternalApis.csproj", "Lazerboard.ExternalApis/"]
COPY ["Lazerboard.Data/Lazerboard.Data.csproj", "Lazerboard.Data/"]
RUN dotnet restore "Lazerboard.ExternalApis/Lazerboard.ExternalApis.csproj"
COPY . .
WORKDIR "/src/Lazerboard.ExternalApis"
RUN dotnet build "./Lazerboard.ExternalApis.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lazerboard.ExternalApis.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lazerboard.ExternalApis.dll"]
