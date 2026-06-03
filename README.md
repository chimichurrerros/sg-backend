# backend

ASPNETCORE_ENVIRONMENT=Development dotnet ef database update
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update

ASPNETCORE_ENVIRONMENT=Production dotnet run --no-launch-profile --configuration Release
