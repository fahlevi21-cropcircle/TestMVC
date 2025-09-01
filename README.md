# TestMVC Project

A .NET 9.0 ASP.NET Core MVC application with LDAP authentication and email functionality.

## Prerequisites

### .NET Framework Information
- **.NET Version**: .NET 9.0
- **Framework**: ASP.NET Core MVC
- **Database**: SQL Server (LocalDB or SQL Server Express)
- **Authentication**: LDAP (OpenLDAP)
- **Email**: SMTP (Mailhog)

### Required Software
1. **.NET 9.0 SDK** - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **Docker Desktop** - Download from [Docker Desktop](https://www.docker.com/products/docker-desktop/)
3. **SQL Server** (LocalDB, Express, or Developer Edition)
4. **Visual Studio 2022** or **Visual Studio Code** (recommended)

## Project Setup

### 1. Clone and Build the Project

```bash
# Clone the repository (if using git)
git clone <repository-url>
cd TestMVC

# Restore dependencies
dotnet restore

# Build the project
dotnet build
```

### 2. Database Setup

The project uses SQL Server with the connection string:
```
Server=CROPCIRCLE-WORK\CROPCIRCLE;Database=LEARNING;Trusted_Connection=True;TrustServerCertificate=True;
```

**For local development, update `appsettings.json`:**
```json
{
    "ConnectionStrings": {
        "Connection": "Server=(localdb)\\mssqllocaldb;Database=TestMVC;Trusted_Connection=True;TrustServerCertificate=True;"
    }
}
```

**Create the database:**
```bash
# Run Entity Framework migrations (if applicable)
dotnet ef database update
```

### 3. LDAP Server Setup (OpenLDAP)

#### Start OpenLDAP Container
```bash
docker run -p 389:389 --name l-ldap \
  -e LDAP_ORGANISATION="TestOrg" \
  -e LDAP_DOMAIN="testorg.local" \
  -e LDAP_ADMIN_PASSWORD=admin \
  -d osixia/openldap
```

#### Create LDAP Directory Structure

Create a directory for LDAP files:
```bash
mkdir LDAP
cd LDAP
```

**Create `ou.ldif` (Organizational Unit):**
```ldif
dn: ou=users,dc=testorg,dc=local
objectClass: organizationalUnit
ou: users
```

**Create `user1.ldif` (Sample User):**
```ldif
dn: uid=john,ou=users,dc=testorg,dc=local
objectClass: inetOrgPerson
objectClass: posixAccount
objectClass: top
cn: John Doe
sn: Doe
uid: john
mail: john@testorg.local
uidNumber: 1000
gidNumber: 1000
homeDirectory: /home/johndoe
userPassword: 123
```

#### Add LDAP Entries

**Add the organizational unit:**
```bash
ldapadd -x -D "cn=admin,dc=testorg,dc=local" -w admin -f /LDAP/ou.ldif -H ldap://localhost -ZZ
```

**Add the user:**
```bash
ldapadd -x -D "cn=admin,dc=testorg,dc=local" -w admin -f /LDAP/user1.ldif -H ldap://localhost -ZZ
```

#### Search LDAP Entries
```bash
ldapsearch -x -H ldap://localhost -b dc=testorg,dc=local -D "cn=admin,dc=testorg,dc=local" -w admin
```

### 4. SMTP Server Setup (Mailhog)

#### Start Mailhog Container
```bash
docker run -p 1025:1025 -p 8025:8025 --name mailhog -d mailhog/mailhog
```

**Mailhog Configuration:**
- **SMTP Port**: 1025 (for sending emails)
- **Web UI Port**: 8025 (for viewing emails)
- **Web UI URL**: http://localhost:8025

The project is already configured to use Mailhog in `appsettings.json`:
```json
{
    "Smtp": {
        "Host": "localhost",
        "Port": 1025,
        "Sender": "no-reply@test.com",
        "SenderName": "System (No-Reply)"
    }
}
```

## Running the Application

### 1. Start Required Services

**Start LDAP Server:**
```bash
docker start l-ldap
```

**Start Mailhog:**
```bash
docker start mailhog
```

### 2. Run the Application

```bash
# Navigate to project directory
cd TestMVC

# Run the application
dotnet run
```

The application will be available at:
- **Main Application**: https://localhost:5001 or http://localhost:5000
- **Mailhog Web UI**: http://localhost:8025

### 3. Access the Application

1. Open your browser and navigate to `https://localhost:5001`
2. You'll be redirected to the login page
3. Use the LDAP credentials created earlier:
   - **Username**: john
   - **Password**: 123

## Project Features

- **Authentication**: LDAP-based user authentication
- **Email Functionality**: SMTP email sending via Mailhog
- **Database**: SQL Server with Entity Framework Core
- **PDF Generation**: QuestPDF integration
- **Excel Export**: ClosedXML integration
- **Scheduled Tasks**: Background service with MyScheduler

## Troubleshooting

### Common Issues

1. **LDAP Connection Failed**
   - Ensure OpenLDAP container is running: `docker ps`
   - Check LDAP port is accessible: `telnet localhost 389`

2. **Database Connection Failed**
   - Verify SQL Server is running
   - Check connection string in `appsettings.json`
   - Ensure database exists

3. **Email Not Sending**
   - Verify Mailhog container is running: `docker ps`
   - Check Mailhog web UI at http://localhost:8025
   - Ensure port 1025 is not blocked

4. **Build Errors**
   - Ensure .NET 9.0 SDK is installed: `dotnet --version`
   - Restore packages: `dotnet restore`
   - Clean and rebuild: `dotnet clean && dotnet build`

### Useful Docker Commands

```bash
# List running containers
docker ps

# Stop containers
docker stop l-ldap mailhog

# Remove containers
docker rm l-ldap mailhog

# View container logs
docker logs l-ldap
docker logs mailhog
```

## Development

### Project Structure
- **Controllers/**: MVC Controllers (Home, Order, Product)
- **Models/**: Data models and view models
- **Views/**: Razor views
- **Data/**: Database context, services, and utilities
- **wwwroot/**: Static files (CSS, JS, images)

### Key Dependencies
- **Microsoft.EntityFrameworkCore**: ORM for database operations
- **MailKit**: Email functionality
- **System.DirectoryServices.Protocols**: LDAP integration
- **QuestPDF**: PDF generation
- **ClosedXML**: Excel file operations

## License

This project is for educational purposes.
