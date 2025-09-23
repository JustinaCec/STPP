# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY ./SchoolHelpDeskAPI.csproj ./
RUN dotnet restore "./SchoolHelpDeskAPI.csproj"

# Copy the rest of the files
COPY . ./
RUN dotnet publish "SchoolHelpDeskAPI.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80

ENTRYPOINT ["dotnet", "SchoolHelpDeskAPI.dll"]
