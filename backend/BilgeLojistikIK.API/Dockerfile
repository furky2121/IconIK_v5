FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/BilgeLojistikIK.API/BilgeLojistikIK.API.csproj", "backend/BilgeLojistikIK.API/"]
RUN dotnet restore "backend/BilgeLojistikIK.API/BilgeLojistikIK.API.csproj"
COPY backend/BilgeLojistikIK.API/ backend/BilgeLojistikIK.API/
WORKDIR "/src/backend/BilgeLojistikIK.API"
RUN dotnet build "BilgeLojistikIK.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BilgeLojistikIK.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory structure
RUN mkdir -p wwwroot/uploads/avatars && \
    mkdir -p wwwroot/uploads/documents && \
    mkdir -p wwwroot/uploads/videos

# Set permissions
RUN chmod -R 755 wwwroot

ENTRYPOINT ["dotnet", "BilgeLojistikIK.API.dll"]
