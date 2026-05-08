# Markit Setup Guide

This guide provides detailed instructions for setting up Markit, including all configuration options and troubleshooting tips.

## Table of Contents

- [Quick Start](#quick-start)
- [Prerequisites](#prerequisites)
- [Setup Methods](#setup-methods)
  - [Automated Setup (Recommended)](#automated-setup-recommended)
  - [Manual Setup](#manual-setup)
- [Configuration Reference](#configuration-reference)
- [Setup Script Options](#setup-script-options)
- [Environment-Specific Setup](#environment-specific-setup)
- [Advanced Configuration](#advanced-configuration)
- [Troubleshooting](#troubleshooting)
- [FAQ](#faq)

## Quick Start

Most of the time, the automated setup is the fastest way to get started:

**Windows:**
```powershell
cd markit
.\scripts\setup.ps1
```

**Linux/Mac:**
```bash
cd markit
chmod +x scripts/setup.sh scripts/run.sh
./scripts/setup.sh
```

## Prerequisites

### Required Tools

| Tool | Minimum Version | Download Link | Purpose |
|------|----------------|---------------|---------|
| .NET SDK | 9.0 | [Download](https://dotnet.microsoft.com/download) | Backend API runtime |
| Node.js | 18.0 | [Download](https://nodejs.org/) | Frontend build tools |
| pnpm | 8.0 | `npm install -g pnpm` | Fast package manager |

### Optional Tools

| Tool | Purpose | Alternative |
|------|---------|-------------|
| Docker | PostgreSQL database (recommended) | Install PostgreSQL manually |

### Verify Prerequisites

Run these commands to check your installed versions:

```bash
dotnet --version    # Should be 9.0.x or higher
node --version      # Should be v18.x or higher
pnpm --version      # Should be 8.x or higher
docker --version    # Optional but recommended
```

## Setup Methods

### Automated Setup (Recommended)

The setup script automates the entire configuration process.

#### Interactive Mode

Run the setup script without any arguments for an interactive experience:

```powershell
# Windows
.\setup.ps1

# Linux/Mac
./setup.sh
```

The script will:
1. Check all prerequisites
2. Prompt for JWT key (with auto-generation option)
3. Ask for admin email and password
4. Configure backend and frontend automatically
5. Start PostgreSQL in Docker
6. Install frontend dependencies
7. Optionally start services

#### Non-Interactive Mode

Provide all values via command-line flags for automation:

```powershell
.\scripts\setup.ps1 `
  -JwtKey "your-64-character-jwt-signing-key-here-must-be-secure" `
  -AdminUser "admin@example.com" `
  -AdminPassword "SecurePass123"
```

#### Flags Reference

**Bash (Linux/Mac):**

| Flag | Short | Description | Default |
|------|-------|-------------|---------|
| `--jwt-key` | `-j` | JWT signing key (64+ chars) | Auto-generated |
| `--admin-user` | `-u` | Admin email/username | `admin@markit.local` |
| `--admin-password` | `-p` | Admin password | None (prompted) |
| `--api-url` | | Backend API URL | `http://localhost:5000` |
| `--spa-url` | | Frontend URL | `http://localhost:4200` |
| `--connection-string` | | PostgreSQL connection | Auto-generated if docker is used |
| `--skip-docker` | | Don't use Docker for PostgreSQL | false |
| `--no-run` | | Setup only, don't start services | false |
| `--help` | | Show help message | - |

**PowerShell (Windows):**

| Flag | Short | Description | Default |
|------|-------|-------------|---------|
| `-JwtKey` | `-j` | JWT signing key (64+ chars) | Auto-generated |
| `-AdminUser` | `-u` | Admin email/username | `admin@markit.local` |
| `-AdminPassword` | `-p` | Admin password | None (prompted) |
| `-ApiUrl` | | Backend API URL | `http://localhost:5000` |
| `-SpaUrl` | | Frontend URL | `http://localhost:4200` |
| `-ConnectionString` | | PostgreSQL connection | Auto-generated if docker is used |
| `-SkipDocker` | | Don't use Docker for PostgreSQL | false |
| `-NoRun` | | Setup only, don't start services | false |
| `-Help` | | Show help message | - |

**Example: Complete non-interactive setup**
```bash
./scripts/setup.sh \
  -j "$(./scripts/generate-jwt-key.sh)" \
  -u "admin@mycompany.com" \
  -p "MySecurePassword123"
```

### Manual Setup

If you prefer to configure Markit manually or need custom configurations:

#### 1. Setup Database

**With Docker (Recommended):**
```bash
docker-compose up -d postgres
```

**Without Docker:**
Install PostgreSQL 17 and create the database:
```sql
CREATE DATABASE markitdb;
```

#### 2. Configure Backend

```bash
cd Backend/markitAPI/API
cp appsettings-template.json appsettings.json
```

Edit `appsettings.json` and configure:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "Host=localhost;Port=5432;Database=markitdb;Username=postgres;Password=postgres"
  },
  "SpaSettings": {
    "baseUrl": "http://localhost:4200"
  },
  "JwtSettings": {
    "Key": "your-generated-64-character-jwt-key-here-keep-it-secure-and-random"
  },
  "UserDefaultSettings": {
    "UserName": "admin@markit.local",
    "Password": "YourSecurePassword123"
  }
}
```

#### 3. Configure Frontend

```bash
cd Frontend
cp src/environments/environment.template.ts src/environments/environment.ts
```

Edit `environment.ts`:
```typescript
export const environment = {
  production: 'False',
  baseApiUrl: 'http://localhost:5000',
};
```

#### 4. Install Dependencies

```bash
cd Frontend
pnpm install
```

#### 5. Run Services

**Backend:**
```bash
cd Backend/markitAPI/API
dotnet run --launch-profile "Development"
```

**Frontend (new terminal):**
```bash
cd Frontend
pnpm start
```

## Docker Database Customization

Markit uses Docker Compose to run PostgreSQL with sensible defaults for local development. You can customize the database settings during setup or by manually creating a `.env` file.

### Customization During Setup

When running the setup script with Docker enabled (default), you'll be prompted individually for each database setting:

```powershell
# Windows
.\setup.ps1

# Linux/Mac
./setup.sh
```

The setup script will ask:

1. **Database name** (default: `markitdb`)
   - Must contain only letters, numbers, underscores, and hyphens
   - Example: `myapp_db`, `markit-dev`

2. **PostgreSQL server user** (default: `postgres`)
   - The PostgreSQL server username for authentication
   - This is the user account configured on the PostgreSQL server
   - Example: `admin`, `dbadmin`, `postgres`

3. **PostgreSQL server password** (default: `postgres`)
   - The password for the PostgreSQL server user account
   - **Important**: Change this for production environments!

4. **Database port** (default: `5432`)
   - Must be between 1 and 65535
   - Port on your machine that maps to PostgreSQL inside Docker
   - Example: `5433` if 5432 is already in use

**Behavior:**
- Press **Enter** for any setting to use the default value
- Provide a custom value to override the default
- If **all** settings use defaults, no `.env` file is created (docker-compose uses built-in defaults)
- If **any** setting is customized, a `.env` file is created with all values

The backend connection string is **automatically generated** to match your Docker configuration.

### Manual .env File Creation

If you want to customize Docker settings without running the setup script, create a `.env` file in the project root:

```bash
# Copy the template
cp docker.env.template .env

# Edit with your values
DB_NAME=markitdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
DB_PORT=5432
```

**Important Notes:**
- The `.env` file is excluded from version control (in `.gitignore`)
- Never commit database passwords to the repository
- The `docker.env.template` file is tracked in git as a reference

**⚠️ Important: Database Initialization**

PostgreSQL only initializes the database with environment variables when the data directory is **empty**. The setup scripts automatically handle this by:
- Removing the existing container and volume before creating a new one
- This ensures your custom database name and password are applied correctly
- **Warning:** This will delete any existing data in the PostgreSQL container

### Connection String Auto-Sync

When using Docker, the setup scripts automatically generate the backend connection string from your Docker configuration:

```
Host=localhost;Port={DB_PORT};Database={DB_NAME};Username={POSTGRES_USER};Password={POSTGRES_PASSWORD}
```

This ensures the backend always connects with the correct credentials.

### Without Docker

To use an external PostgreSQL installation instead of Docker:

```powershell
# Windows
.\setup.ps1 -SkipDocker -ConnectionString "Host=myserver;Port=5432;Database=markitdb;Username=user;Password=pass"

# Linux/Mac
./setup.sh --skip-docker --connection-string "Host=myserver;Port=5432;Database=markitdb;Username=user;Password=pass"
```

The Docker customization prompts will be skipped, and you must provide your own connection string.

## Configuration Reference

### Minimal Required Configuration

Only **3 values** are absolutely required to run Markit:

1. **JwtSettings.Key** - Cryptographic key for signing tokens (64+ characters recommended)
2. **UserDefaultSettings.UserName** - Admin email address
3. **UserDefaultSettings.Password** - Admin password (min 8 chars, must contain uppercase, lowercase, digit)

All other settings have sensible defaults.

### Default Values

The setup script uses these defaults (automatically filled):

| Setting | Default Value | Override Flag |
|---------|---------------|---------------|
| ConnectionString | `Host=localhost;Port=5432;Database=markitdb;Username=postgres;Password=postgres` | `--connection-string` |
| SpaSettings.baseUrl | `http://localhost:4200` | `--spa-url` |
| Backend API URL | `http://localhost:5000` | `--api-url` |
| Frontend production mode | `False` | N/A |

### JWT Key Generation

The JWT key is used to sign authentication tokens and must be kept secret.

**Auto-generate (recommended):**
The setup script can generate a secure 64-character key automatically.

**Manual generation:**
```powershell
# Windows
.\scripts\generate-jwt-key.ps1

# Linux/Mac
./scripts/generate-jwt-key.sh
```

**Best practices:**
- Minimum 32 characters
- Recommended 64+ characters
- Use random, unpredictable values
- Never commit to version control

### Password Requirements

Admin password must meet these requirements:
- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)

### Connection Strings

PostgreSQL connection string format:
```
Host=<hostname>;Port=<port>;Database=<database>;Username=<user>;Password=<password>
```

**Default (Docker):**
```
Host=localhost;Port=5432;Database=markitdb;Username=postgres;Password=postgres
```

**Custom database server:**
```
Host=db.mycompany.com;Port=5432;Database=markitdb;Username=markit_user;Password=secure_pass
```

**Local PostgreSQL (non-Docker):**
```
Host=localhost;Port=5432;Database=markitdb;Username=postgres;Password=your_password
```

## Setup Script Options

### Using Flags

All setup script flags can be combined:

```powershell
.\setup.ps1 `
  -j "64-char-jwt-key" `
  -u "admin@example.com" `
  -p "SecurePass123" `
  -ApiUrl "http://localhost:5001" `
  -SpaUrl "http://localhost:4201" `
  -SkipDocker `
  -ConnectionString "Host=mydb;Port=5432;Database=markitdb;Username=user;Password=pass"
```

### Skip Docker Setup

If you already have PostgreSQL installed or want to use an external database:

```powershell
# PowerShell (Windows)
.\setup.ps1 -SkipDocker

# Bash (Linux/Mac)
./setup.sh --skip-docker
```

The script will:
- ⚠️ Warn that default connection string will be used
- Prompt for a custom connection string (optional)
- Show instructions for creating the database manually
- Skip Docker container startup

### Setup Without Running

To configure without starting services:

```powershell
# PowerShell (Windows)
.\setup.ps1 -NoRun

# Bash (Linux/Mac)
./setup.sh --no-run
```

Then start services later:

#### Run Both Services Together (Recommended)

```powershell
# PowerShell (Windows)
.\run.ps1 -All

# Bash (Linux/Mac)
./run.sh --all
```

This automatically:
- Detects the best available terminal (Windows Terminal, gnome-terminal, macOS Terminal, tmux)
- Launches both services in split panes or separate windows
- Starts backend and frontend in parallel for faster startup

#### Run Services Individually

```powershell
# Run interactively (menu-driven)
.\run.ps1      # Windows
./run.sh       # Linux/Mac

# Run specific service
.\run.ps1 -Backend     # Backend only
.\run.ps1 -Frontend    # Frontend only
```

## Environment-Specific Setup

### Development Environment

Default configuration is optimized for development:
- CORS enabled for localhost
- Swagger UI enabled
- Detailed logging
- Hot reload enabled

No additional configuration needed.

### Staging/Production

For non-local environments:

1. Use secure, randomly generated JWT keys
2. Configure proper database connection strings
3. Set strong admin passwords
4. Update CORS settings to match your domain
5. Configure OAuth providers (Google, GitHub) if needed
6. Review rate limiting settings

**Example production setup:**
```powershell
.\setup.ps1 `
  -j "$(.\scripts\generate-jwt-key.ps1)" `
  -u "admin@company.com" `
  -p "VerySecurePassword123!" `
  --api-url "https://api.mycompany.com" `
  -ApiUrl "https://api.mycompany.com" `
  -SpaUrl "https://markit.mycompany.com" `
  -ConnectionString "Host=prod-db.mycompany.com;Port=5432;Database=markitdb;Username=markit_prod;Password=prod_password"
```

## Advanced Configuration

### OAuth Providers (Optional)
Markit can use OAuth providers to allow accessing an **existing markit account**.

#### Google Authentication

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Setup a new project
3. Add to `appsettings.json`:

```json
"GoogleAuthSettings": {
  "ClientId": "your-client-id.apps.googleusercontent.com",
  "ClientSecret": "your-client-secret"
}
```
4. Enable Google as authentication method in the settings section (settings/authentication) of markit using an administrator user.

#### GitHub Authentication

1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Create a GitHub OAuth App
3. Add to `appsettings.json`:

```json
"GitHubAuthSettings": {
  "AppName": "my-app-name",
  "ClientId": "your-github-client-id",
  "ClientSecret": "your-github-client-secret"
}
```
4. Enable GitHub as authentication method in the settings section (settings/authentication) of markit using an administrator user.

> **⚠️ Important**: Markit supports GitHub Gist attachments via the GitHub API. While anonymous access is available by default, it is strongly recommended configuring GitHub authentication. Integrating a GitHub App ensures higher rate limits and a more seamless experience for users.

### Rate Limiting

Adjust in `appsettings.json` if needed:

```json
"RateLimiting": {
  "VolumeControl": {
    "PermitLimit": 300,
    "WindowMinutes": 1,
    "WindowSegments": 4
  },
  "ConcurrencyControl": {
    "PermitLimit": 30,
    "QueueLimit": 15
  }
}
```

### Database Migrations

Migrations are applied automatically on first run. To manage manually:

```bash
cd Backend/markitAPI/markit.Infrastructure

# Apply migrations
dotnet ef database update --startup-project ../API
```

## Troubleshooting

### Setup Script Issues

#### "Prerequisites not met"

**Problem:** Missing required tools.

**Solution:**
1. Install .NET SDK 9.0+
2. Install Node.js 18+
3. Install pnpm: `npm install -g pnpm`
4. Verify: `dotnet --version`, `node --version`, `pnpm --version`

#### "Docker not found"

**Problem:** Docker is not installed or not running.

**Solution:**
- Install Docker Desktop from https://www.docker.com/get-started
- Start Docker Desktop
- Or use `-SkipDocker` (PowerShell) or `--skip-docker` (bash) flag to use existing PostgreSQL

### Database Issues

#### "Cannot connect to database"

**With Docker:**
```bash
# Check if container is running
docker ps | grep markit-postgres

# View container logs
docker-compose logs postgres

# Restart container
docker-compose restart postgres
```

**Without Docker:**
```bash
# Check PostgreSQL status (Linux)
sudo systemctl status postgresql

# Check PostgreSQL status (Mac)
brew services list | grep postgresql

# Check PostgreSQL status (Windows)
Get-Service -Name postgresql*
```

#### "Database does not exist"

Create it manually:
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE markitdb;

# Verify
\l
\q
```

### Port Conflicts

#### Port 5000 already in use (Backend)

Change in `Backend/markitAPI/API/Properties/launchSettings.json`:
```json
"applicationUrl": "http://localhost:5001"
```

Or use setup flag: `--api-url http://localhost:5001`

#### Port 4200 already in use (Frontend)

Run with custom port:
```bash
cd Frontend
pnpm start -- --port 4201
```

Or use setup flag: `--spa-url http://localhost:4201`

### Running Multiple Services

#### Split panes not working with `-All` / `--all` flag

**Windows:**
- **Problem:** Windows Terminal not detected or split panes don't open
- **Solution:** 
  - Install Windows Terminal from Microsoft Store (recommended for Windows 11)
  - Or: Script will automatically fall back to separate PowerShell windows
  - Or: Run services individually: `.\run.ps1 -Backend` and `.\run.ps1 -Frontend` in separate terminals

**Linux:**
- **Problem:** gnome-terminal or tmux not available
- **Solution:**
  - Install gnome-terminal: `sudo apt install gnome-terminal` (Ubuntu/Debian)
  - Or install tmux: `sudo apt install tmux`
  - Or: Script will attempt to use xterm as fallback
  - Or: Run services individually: `./run.sh --backend` and `./run.sh --frontend`

**macOS:**
- **Problem:** Terminal.app not opening tabs/windows
- **Solution:**
  - Ensure Terminal.app has proper permissions in System Preferences → Security & Privacy
  - Or: Run services individually in separate Terminal tabs manually

#### Services fail to start in split panes

**Problem:** Windows Terminal opens but services don't start

**Solution:**
1. Check that configuration files exist:
   - `Backend/markitAPI/API/appsettings.json`
   - `Frontend/src/environments/environment.ts`
2. Run setup if missing: `.\setup.ps1` or `./setup.sh`
3. Verify Docker container is running: `docker ps | grep markit-postgres`
4. Check terminal output for specific error messages

#### Can't close services started with `-All`

**Windows Terminal (split panes):**
- Press `Ctrl+C` in each pane to stop the corresponding service
- Or close the entire Windows Terminal window to stop all services

**Separate Windows:**
- Close each PowerShell/terminal window individually
- Or press `Ctrl+C` in each window

### Configuration Issues

#### "JWT key too short"

Minimum 32 characters required. Use the generator:
```bash
.\scripts\generate-jwt-key.ps1  # Windows
./scripts/generate-jwt-key.sh   # Linux/Mac
```

#### "Invalid admin password"

Ensure password meets requirements:
- Minimum 8 characters
- Contains uppercase letter
- Contains lowercase letter
- Contains digit

#### "CORS error in browser"

Verify `SpaSettings.baseUrl` in `appsettings.json` matches your frontend URL:
```json
"SpaSettings": {
  "baseUrl": "http://localhost:4200"
}
```

Restart backend after changes.

## FAQ

### Do I need Docker?

No, but it's recommended. Docker simplifies PostgreSQL setup. Without Docker, you need to:
- Install PostgreSQL 17 manually
- Create the `markitdb` database
- Configure connection string
- Manage PostgreSQL service yourself

### Can I use a different database?

Markit is designed for PostgreSQL 17. For now, using other databases requires code changes.

### Where are the logs stored?

Backend logs: `Backend/markitAPI/API/Logs/applog-YYYYMMDD.txt`

View latest:
```bash
# Windows
Get-Content Backend\markitAPI\API\Logs\applog-*.txt -Tail 50

# Linux/Mac
tail -f Backend/markitAPI/API/Logs/applog-*.txt
```

### Can I change the admin credentials later?

Yes, after first login:
1. Log in with initial admin credentials
2. Go to settings/accounts
3. Update your profile and password

### How do I add more users?

After logging in as admin:
1. Navigate to settings/accounts
2. Click "Create account"
3. Fill in user details
4. Assign appropriate role (Admin, General, Demo)

### What are the different user roles?

- **Admin**: Full system access, user management
- **General**: Standard user access
- **Demo**: Limited access for demo purposes

### How do I configure OAuth (Google/GitHub)?

See [OAuth Providers](#oauth-providers-optional) section above.

### Can I run multiple instances?

Yes, use different ports for each instance:
```powershell
.\setup.ps1 --api-url http://localhost:5001 --spa-url http://localhost:4201
```

### How do I update Markit?

```bash
git pull origin main
cd Frontend
pnpm install  # Update frontend dependencies
cd ../Backend/markitAPI/API
dotnet restore  # Update backend dependencies
dotnet run  # Migrations apply automatically
```

### How do I deploy to production?
- See [Environment-Specific Setup](#environment-specific-setup) section above.
- There is a CI/CD implementation example in `.github/workflows/deploy.yml` to build the components of the app and deploy them in a VPS.

For manual deployment:
1. Build frontend: `cd Frontend && pnpm build:production`
2. Publish backend: `cd Backend/markitAPI/API && dotnet publish -c Release`
3. Configure production database
4. Set production environment variables
5. Deploy to your hosting provider

---
