# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["glms/glms.csproj", "glms/"]
COPY ["glms.Api/glms.Api.csproj", "glms.Api/"]
RUN dotnet restore "glms/glms.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/glms"
RUN dotnet publish "glms.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "glms.dll"]
