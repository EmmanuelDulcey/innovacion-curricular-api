# =============================================================================
# Dockerfile - API Innovacion Curricular (ASP.NET Core, puerto 8036)
# Imagen basada en el SDK de .NET, ejecutando dotnet watch con la URL en 8036.
# Fuente: 8_tasks.md Fase 6 y 3_plan.md Seccion 5.
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8036
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8036

COPY ApiInnovacion.csproj .
RUN dotnet restore

COPY . .

ENTRYPOINT ["dotnet", "watch", "--no-launch-profile", "run", "--project", "ApiInnovacion.csproj"]