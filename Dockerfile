# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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
EXPOSE 80

WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS="http://+"

ENTRYPOINT ["dotnet", "eKIBRA.Web.dll"]