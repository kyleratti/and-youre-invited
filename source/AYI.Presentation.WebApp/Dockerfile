FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AYI.Presentation.WebApp/AYI.Presentation.WebApp.csproj", "AYI.Presentation.WebApp/"]
RUN dotnet restore "AYI.Presentation.WebApp/AYI.Presentation.WebApp.csproj"
COPY . .
WORKDIR "/src/AYI.Presentation.WebApp"
RUN dotnet build "AYI.Presentation.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AYI.Presentation.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AYI.Presentation.WebApp.dll"]
