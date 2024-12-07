#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["api.taassistant/api.taassistant.csproj", "api.taassistant/"]
COPY ["taassistant.model/taassistant.model.csproj", "taassistant.model/"]
COPY ["taassistant.interfaces/taassistant.interfaces.csproj", "taassistant.interfaces/"]
COPY ["taassistant.data/taassistant.data.csproj", "taassistant.data/"]
COPY ["taassistant.service/taassistant.service.csproj", "taassistant.service/"]
RUN dotnet restore "./api.taassistant/api.taassistant.csproj"
COPY . .
WORKDIR "/src/api.taassistant"
RUN dotnet build "./api.taassistant.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./api.taassistant.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.taassistant.dll"]