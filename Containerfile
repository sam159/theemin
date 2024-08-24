FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /app

COPY . .
RUN dotnet restore
RUN dotnet publish xweb -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=builder /app/xweb/content ./content
COPY --from=builder /app/xweb/templates ./templates
COPY --from=builder /app/out .

VOLUME ["/app/wwwdata"]
VOLUME ["/app/content"]
VOLUME ["/app/templates"]

ENTRYPOINT ["dotnet", "/app/xweb.dll"]
