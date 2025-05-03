FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eKIBRA.Web/eKIBRA.Web.csproj", "eKIBRA.Web/"]
RUN dotnet restore "eKIBRA.Web/eKIBRA.Web.csproj"
COPY "src/eKIBRA.Web" "src/eKIBRA.Web"
WORKDIR "/src/eKIBRA.Web"
RUN dotnet build "./eKIBRA.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eKIBRA.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eKIBRA.Web.dll"]
