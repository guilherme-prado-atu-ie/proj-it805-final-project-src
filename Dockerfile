# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
EXPOSE 8080
EXPOSE 8081
WORKDIR /src

# copy csproj and restore as distinct layers
# COPY *.sln .
COPY eKIBRA.Web/*.csproj ./eKIBRA.Web/
RUN dotnet restore eKIBRA.Web/eKIBRA.Web.csproj

# copy everything else and build app
COPY /eKIBRA.Web/. ./eKIBRA.Web/
WORKDIR /src/eKIBRA.Web
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
USER $APP_UID
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "eKIBRA.Web.dll"]