# Database Setup - Enflorarte Topi Proyecto

This project uses **SQL Server only**.

---

## Prerequisites
- SQL Server running (local install or Docker)
- SQL Server credentials available

---

## Configuration Steps

1. **Restore NuGet packages** (if not already done):
   ```bash
   dotnet restore
   ```

2. **Create or update `appsettings.Development.json`**:

   In the `EnflorarteTopiProyecto` folder, set the connection string:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
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

3. **Run the project**:
   ```bash
   dotnet run
   ```

The app will create the database if it does not exist and apply pending migrations on startup.

---

## Important Notes

⚠️ **DO NOT commit `appsettings.Development.json` with your actual passwords to the repository!**

Add to `.gitignore` if not already present:
```
appsettings.Development.json
```

✅ **Best Practice**: Each team member should have their own `appsettings.Development.json` file configured for their local environment.

---

## Troubleshooting

### SQL Server Connection Issues
- Ensure SQL Server is running
- Check SQL Server is set to accept TCP connections
- Verify credentials and server name
- Try `TrustServerCertificate=True` if certificate errors occur
