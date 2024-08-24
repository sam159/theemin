FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /app

COPY .  ./
RUN dotnet restore
RUN dotnet publish Theemin -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

# add curl for healtchecks
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=builder /app/Theemin/content ./content
COPY --from=builder /app/Theemin/templates ./templates
COPY --from=builder /app/out .
COPY LICENSE /app
COPY README.md /app

VOLUME ["/app/wwwdata"]
VOLUME ["/app/content"]
VOLUME ["/app/templates"]

EXPOSE 8080
ENTRYPOINT ["dotnet", "/app/Theemin.dll"]

HEALTHCHECK CMD curl --fail http://127.0.0.1:8080/healthz || exit
