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
EXPOSE 8080
EXPOSE 8081

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

WORKDIR /app
COPY --link aspnetapp.pfx /app
COPY --link --from=build /app ./

ENV \
    ASPNETCORE_Kestrel__Certificates__Default__Path="/app/aspnetapp.pfx"
#    ASPNETCORE_URLS="https://+:443;http://+80"    
#    ASPNETCORE_Kestrel__Certificates__Default__Password="-\0pw-" \

HEALTHCHECK CMD curl --fail --silent --show-error localhost:8080 || exit 1

USER $APP_UID
ENTRYPOINT ["dotnet", "eKIBRA.Web.dll"]