#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.BrandGameCredits.Notification/Tgs.BrandGameCredits.Notification.csproj", "Tgs.BrandGameCredits.Notification/"]
COPY ["Tgs.BrandGameCredits.Notification.Core/Tgs.BrandGameCredits.Notification.Core.csproj", "Tgs.BrandGameCredits.Notification.Core/"]
COPY ["Tgs.BrandGameCredits.Notification.Repositories/Tgs.BrandGameCredits.Notification.Repositories.csproj", "Tgs.BrandGameCredits.Notification.Repositories/"]
RUN dotnet restore "./Tgs.BrandGameCredits.Notification/./Tgs.BrandGameCredits.Notification.csproj"
COPY . .
WORKDIR "/src/Tgs.BrandGameCredits.Notification"
RUN dotnet build "./Tgs.BrandGameCredits.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.BrandGameCredits.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.BrandGameCredits.Notification.dll"]