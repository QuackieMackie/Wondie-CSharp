# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

LABEL author="QuackieMackie"

# Copy the project file and restore dependencies
COPY ["Wondie-CSharp.csproj", "./"]
RUN dotnet restore "./Wondie-CSharp.csproj" --verbosity detailed

# Copy the rest of the code
COPY . .

# Publish the application
RUN dotnet publish "./Wondie-CSharp.csproj" -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Expose port 5000
EXPOSE 5000

# Create the logs directory and set permissions to ensure it is writable
RUN mkdir -p /app/logs && chmod -R 777 /app/logs

# Copy the published app from the build stage
COPY --from=build /app/out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Wondie.dll"]
