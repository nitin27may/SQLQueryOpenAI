FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000
RUN groupadd -g 2000 dotnet \
    && useradd -m -u 2000 -g 2000 dotnet
USER dotnet

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG DOTNET_SKIP_POLICY_LOADING=true
WORKDIR /src
COPY ["AIQueryBuilder.csproj", "."]

RUN dotnet restore "./AIQueryBuilder.csproj"
COPY . .
RUN dotnet build "./AIQueryBuilder.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN ls /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AIQueryBuilder.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
# COPY --from=publish /app/publish/Contact.Api.dll .
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIQueryBuilder.dll"]
