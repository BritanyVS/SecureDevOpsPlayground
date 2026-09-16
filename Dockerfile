# ============================================================
# Dockerfile del backend (SecureDevOps.API) — versión SEGURA
# Ídem docker/Dockerfile.backend (mantenido en sincronía).
# Jenkinsfile construye con este archivo.
#  - Multi-stage build (sdk → aspnet runtime)
#  - Usuario NO-root (`app`)
#  - Sin secretos en el Dockerfile
#  - Health check /health
# ============================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SecureDevOps.API/SecureDevOps.API.csproj", "SecureDevOps.API/"]
RUN dotnet restore "SecureDevOps.API/SecureDevOps.API.csproj"

COPY SecureDevOps.API/ ./SecureDevOps.API/
WORKDIR "/src/SecureDevOps.API"
RUN dotnet publish "SecureDevOps.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
RUN mkdir -p /app/data /app/uploads && chown -R app:app /app/data /app/uploads

COPY --from=build /app/publish .

USER app

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/SecureDevOpsDb.db"
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=20s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SecureDevOps.API.dll"]