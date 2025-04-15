# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update
RUN apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs

FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Maboroshi.Web/Maboroshi.Web.csproj", "Maboroshi.Web/"]
COPY ["Maboroshi.ReactClient/maboroshi.reactclient.esproj", "maboroshi.reactclient/"]
RUN dotnet restore "./Maboroshi.Web/Maboroshi.Web.csproj"
COPY . .
WORKDIR "/src/Maboroshi.Web"

# Copy React dist output into wwwroot
RUN mkdir -p ./wwwroot && cp -r ../Maboroshi.ReactClient/dist/* ./wwwroot/

RUN dotnet build "./Maboroshi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Maboroshi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Maboroshi.Web.dll"]