FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lazerboard.Data/Lazerboard.Data.csproj", "Lazerboard.Data/"]
RUN dotnet restore "Lazerboard.Data/Lazerboard.Data.csproj"
COPY . .
WORKDIR "/src/Lazerboard.Data"
RUN dotnet build "./Lazerboard.Data.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lazerboard.Data.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lazerboard.Data.dll"]
