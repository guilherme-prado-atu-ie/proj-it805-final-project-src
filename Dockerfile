# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY --link eKIBRA.Web/*.csproj ./eKIBRA.Web/
RUN dotnet restore eKIBRA.Web/eKIBRA.Web.csproj  \
    --use-current-runtime

# copy everything else and build app
COPY --link /eKIBRA.Web/. ./eKIBRA.Web/

WORKDIR /src/eKIBRA.Web
RUN dotnet publish -c release -o /app \
    --no-restore  \
    --use-current-runtime

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
EXPOSE 80/tcp
EXPOSE 443/tcp

# https://github.com/dotnet/dotnet-docker/blob/main/samples/aspnetapp/Dockerfile.alpine-icu
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --upgrade --no-cache \
    curl \
    icu-data-full \
    icu-libs \
    tzdata

COPY cert.crt /https/cert.crt
COPY cert.key /https/cert.key
    
WORKDIR /app
COPY --link --from=build /app ./
 
ENV \
    ASPNETCORE_Kestrel__Certificates__Default__Path="/https/cert.crt" \
    ASPNETCORE_Kestrel__Certificates__Default__KeyPath="/https/cert.key" \
    ASPNETCORE_HTTP_PORTS="80" \
    ASPNETCORE_HTTPS_PORTS="443"

HEALTHCHECK CMD curl --fail --silent --show-error localhost || exit 1

USER $APP_UID
ENTRYPOINT ["./eKIBRA.Web"]