# Multi-Database Setup Guide - Enflorarte Topi Proyecto

This project supports both **PostgreSQL** and **SQL Server**. Follow the instructions below based on your preference.

---

## Option 1: PostgreSQL Setup (Recommended for Development)

### Prerequisites
- PostgreSQL installed and running
- Database credentials available

### Configuration Steps

1. **Restore NuGet packages** (if not already done):
   ```bash
   dotnet restore
   ```

2. **Update your local `appsettings.Development.json`**:
   
   In the `EnflorarteTopiProyecto` folder, ensure your `appsettings.Development.json` has:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "DatabaseType": "postgres",
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=enflorarte_topi_proyecto;Username=postgres;Password=YOUR_PASSWORD"
     }
   }
   ```

   **Replace `YOUR_PASSWORD` with your PostgreSQL password.**

3. **Create the database** (using psql or your PostgreSQL client):
   ```sql
   CREATE DATABASE enflorarte_topi_proyecto;
   ```

4. **Apply migrations**:
   ```bash
   dotnet ef database update
   ```

5. **Run the project**:
   ```bash
   dotnet run
   ```

---

## Option 2: SQL Server Setup

### Prerequisites
- SQL Server running (SQL Server Express for development is fine)
- SQL Server credentials available

### Configuration Steps

1. **Restore NuGet packages** (if not already done):
   ```bash
   dotnet restore
   ```

2. **Update your local `appsettings.Development.json`**:
   
   In the `EnflorarteTopiProyecto` folder, create or update `appsettings.Development.json` with:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "DatabaseType": "sql",
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER,1433;Database=EnflorarteTopiProyectoDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
     }
   }
   ```

   **Replace:**
   - `YOUR_SERVER` with your SQL Server name or IP
   - `YOUR_PASSWORD` with your SA password

   **Example for localhost**:
   ```json
   "DefaultConnection": "Server=localhost,1433;Database=EnflorarteTopiProyectoDb;User Id=sa;Password=MyPassword123;TrustServerCertificate=True"
   ```

3. **Create the database** (if needed):
   - Use SQL Server Management Studio, or
   - Run SQL script from `Schema.sql` file in the project

4. **Apply migrations**:
   ```bash
   dotnet ef database update
   ```

5. **Run the project**:
   ```bash
   dotnet run
   ```

---

## Key Configuration Files

### DatabaseType Values
- `"sql"` → Uses SQL Server (default in production)
- `"postgres"` → Uses PostgreSQL

### Connection Process
1. Application reads `DatabaseType` setting
2. Reads the `DefaultConnection` connection string
3. Matches the database provider (SQL Server or PostgreSQL)
4. Establishes connection

### File Hierarchy
- `appsettings.json` - Production defaults (currently set to SQL Server)
- `appsettings.Development.json` - Local overrides for development

---

## Important Notes

⚠️ **DO NOT commit `appsettings.Development.json` with your actual passwords to the repository!**

Add to `.gitignore` if not already present:
```
appsettings.Development.json
```

✅ **Best Practice**: Each team member should have their own `appsettings.Development.json` file configured for their local environment.

---

## Switching Between Databases

To switch your local development environment between PostgreSQL and SQL Server:

1. Edit `appsettings.Development.json`
2. Change `"DatabaseType"` value
3. Update `"DefaultConnection"` connection string accordingly
4. Restart the application

---

## Troubleshooting

### PostgreSQL Connection Issues
- Ensure PostgreSQL is running: `pg_isready`
- Check port 5432 is accessible
- Verify username/password in connection string

### SQL Server Connection Issues
- Ensure SQL Server is running
- Check SQL Server is set to accept TCP connections
- Verify credentials and server name
- Try `TrustServerCertificate=True` if certificate errors occur

### Migration Issues
- Delete your database and recreate it
- Run: `dotnet ef database update`
- If that doesn't work, try: `dotnet ef database drop` then `dotnet ef database update`

---

## Managing Migrations

The project uses Entity Framework Core migrations. When the database schema changes:

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply pending migrations
dotnet ef database update

# Revert last migration
dotnet ef database update -1
```

Both PostgreSQL and SQL Server will automatically apply the same migrations correctly.

---

## Questions?

Check the [OpcionBd.cs](./OpcionBd.cs) file to see how database selection works.
