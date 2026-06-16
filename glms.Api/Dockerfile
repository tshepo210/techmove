# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["glms.Api/glms.Api.csproj", "glms.Api/"]
COPY ["glms/glms.csproj", "glms/"]
RUN dotnet restore "glms.Api/glms.Api.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/glms.Api"
RUN dotnet publish "glms.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "glms.Api.dll"]
