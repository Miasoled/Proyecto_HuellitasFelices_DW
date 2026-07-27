FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HuellitasFelices.csproj", "./"]
RUN dotnet restore "HuellitasFelices.csproj"

COPY . .
RUN dotnet publish "HuellitasFelices.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/keys \
    && chmod +x /usr/local/bin/docker-entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
