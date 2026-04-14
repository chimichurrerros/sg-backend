# Comando para traer cosas de la db

ASPNETCORE_ENVIRONMENT=Development dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Models --context-dir Infrastructure/Context --context AppDbContext --schema public --force
