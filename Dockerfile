
# Phase de publication
FROM build AS publish
WORKDIR /src
# Publication des fichiers nécessaires pour l'exécution
RUN dotnet publish "Authentifications/Authentifications.csproj" -c $BUILD_CONFIGURATION -o /app/publish || { echo 'dotnet publish failed'; exit 1; }

# Phase finale d'exécution (RUNTIME)
FROM base AS runtime
WORKDIR /source

# Copier les fichiers publiés depuis la phase de publication
COPY --from=publish /app/publish .

# Copier les fichiers de configuration (y compris appsettings.*)
COPY Authentifications/appsettings.* .

# Copier le certificat nécessaire
COPY TasksApi.pfx /etc/ssl/certs/TasksApi.pfx

# Configuration des variables d'environnement
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/etc/ssl/certs/TasksApi.pfx
# Check plustaard
ENV ASPNETCORE_ENVIRONMENT=Development 
ENV ASPNETCORE_URLS=https://+:7103

# Point d'entrée
ENTRYPOINT ["dotnet", "Authentifications.dll"]