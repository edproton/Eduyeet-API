# Use the official .NET 8 SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Application.UnitTests/Application.UnitTests.csproj", "Application.UnitTests/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infra/Infra.csproj", "Infra/"]
RUN dotnet restore "API/API.csproj"

# Copy the rest of the files
COPY . .

# Build and publish
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Copy the placeholder replacement script
COPY replace_placeholders.sh .
RUN chmod +x replace_placeholders.sh

# Set the script as the entrypoint
ENTRYPOINT ["./replace_placeholders.sh"]