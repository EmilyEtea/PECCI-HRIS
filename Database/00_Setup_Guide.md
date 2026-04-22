# PECCI HRIS — SQL Server Setup Guide

## Step 1: Configure Connection String

Open `appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER\\INSTANCE;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Common connection string formats:

| Scenario | Connection String |
|---|---|
| Local SQL Express | `Server=localhost\\SQLEXPRESS;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| Local default instance | `Server=localhost;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| Named instance | `Server=PCNAME\\SQLEXPRESS;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Auth (username/password) | `Server=localhost;Database=PECCI_HRIS_DB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |

## Step 2: Run Migrations (Entity Framework)

Open **Package Manager Console** in Visual Studio:

```powershell
# Create the initial migration
Add-Migration InitialCreate

# Apply to database (creates PECCI_HRIS_DB automatically)
Update-Database
```

Or via CLI:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Step 3: Verify in SSMS

1. Open SQL Server Management Studio
2. Connect to your server
3. Expand Databases → you should see `PECCI_HRIS_DB`
4. Expand Tables → verify all tables are created

## Step 4: Default Login

After migration, log in with:
- **Username:** `admin`
- **Password:** `Admin@123`

> Change this password immediately after first login!

## Troubleshooting

**"Cannot open database" error:**
- Ensure SQL Server service is running (Services → SQL Server)
- Check server name in connection string

**"Login failed" error:**
- If using Windows Auth: ensure your Windows account has SQL Server access
- If using SQL Auth: verify username/password

**"Network-related error":**
- Enable TCP/IP in SQL Server Configuration Manager
- Check firewall rules for port 1433
