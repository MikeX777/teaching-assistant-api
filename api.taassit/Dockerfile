#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["api.taassit/api.taassit.csproj", "api.taassit/"]
COPY ["taassist.model/taassist.model.csproj", "taassist.model/"]
COPY ["taassit.service/taassist.service.csproj", "taassist.service/"]
RUN dotnet restore "./api.taassit/api.taassit.csproj"
COPY . .
WORKDIR "/src/api.taassit"
RUN dotnet build "./api.taassit.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./api.taassit.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.taassit.dll"]