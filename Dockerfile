# ============================================================
# BACKEND (.NET 8) — LABORATORIO Snyk Container
# ⚠️ VULNERABLE A PROPÓSITO (fácil de corregir, ver docs/VULNERABILITIES.md):
#   1. Runtime = imagen SDK (trae build tools y muchos CVEs agregados). FIX: aspnet:8.0.
#   2. Corre como ROOT (sin USER). FIX: USER app.
#   3. Sin HEALTHCHECK. FIX: healthcheck con /health.
# También lo usa Render para desplegar el backend (build automático por push).
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src
COPY SecureDevOps.API/ ./SecureDevOps.API/
RUN dotnet publish "SecureDevOps.API/SecureDevOps.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

WORKDIR /app/publish
RUN mkdir -p /app/data /app/uploads

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/SecureDevOpsDb.db"
EXPOSE 8080

CMD ["dotnet", "SecureDevOps.API.dll"]