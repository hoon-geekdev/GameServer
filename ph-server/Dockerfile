# docker build -t ph-server:latest .
# docker run -d -p 8080:8080 ph-server:latest

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project files to the container
COPY . ./

# Restore and build the project
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 8080
# ENV ASPNETCORE_URLS http://*:8080

# Run the application
ENTRYPOINT ["dotnet", "ph-server.dll"]