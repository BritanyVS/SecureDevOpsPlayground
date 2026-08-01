FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SecureDevOps.API/SecureDevOps.API.csproj", "SecureDevOps.API/"]
RUN dotnet restore "SecureDevOps.API/SecureDevOps.API.csproj"

COPY . .
WORKDIR "/src/SecureDevOps.API"
RUN dotnet build "SecureDevOps.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecureDevOps.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecureDevOps.API.dll"]