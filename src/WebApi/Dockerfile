# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first so restore can be cached in its own layer.
COPY CleanArchitectureCqrsTemplate.sln ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/
COPY tests/Application.Tests/Application.Tests.csproj tests/Application.Tests/

RUN dotnet restore CleanArchitectureCqrsTemplate.sln

# Copy everything else and publish.
COPY . .
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
