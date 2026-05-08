#!/usr/bin/env pwsh
# Markit Setup Script for Windows
# This script automates the setup of Markit environment

param(
    [Alias("j")][string]$JwtKey = "",
    [Alias("u")][string]$AdminUser = "",
    [Alias("p")]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingPlainTextForPassword', '', Justification='Required for non-interactive mode via command-line argument')]
    [string]$AdminPassword = "",
    [string]$ApiUrl = "http://localhost:5000",
    [string]$SpaUrl = "http://localhost:4200",
    [string]$ConnectionString = "",
    [switch]$SkipDocker,
    [switch]$NoRun,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# Constants
$SCRIPT_DIR = Split-Path -Parent $PSScriptRoot
$BACKEND_DIR = Join-Path $SCRIPT_DIR "Backend\markitAPI\API"
$FRONTEND_DIR = Join-Path $SCRIPT_DIR "Frontend"
$APPSETTINGS_TEMPLATE = Join-Path $BACKEND_DIR "appsettings-template.json"
$APPSETTINGS_FILE = Join-Path $BACKEND_DIR "appsettings.json"
$ENV_TEMPLATE = Join-Path $FRONTEND_DIR "src\environments\environment.template.ts"
$ENV_FILE = Join-Path $FRONTEND_DIR "src\environments\environment.ts"
$DEFAULT_CONNECTION_STRING = "Host=localhost;Port=5432;Database=markitdb;Username=postgres;Password=postgres"

# Helper Functions
function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Text)
    Write-Host "[OK] $Text" -ForegroundColor Green
}

function Write-Error {
    param([string]$Text)
    Write-Host "[ERROR] $Text" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Text)
    Write-Host "[WARNING] $Text" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Text)
    Write-Host "[INFO] $Text" -ForegroundColor Cyan
}

function Show-Help {
    Write-Host @"

Markit Setup Script - Automates environment setup

USAGE:
    .\setup.ps1 [OPTIONS]

OPTIONS:
    -JwtKey, -j <key>              JWT signing key (auto-generated if not provided)
    -AdminUser, -u <email>         Admin username/email (default: admin@markit.local)
    -AdminPassword, -p <password>  Admin password (required, must be strong)
    -ApiUrl <url>                  Backend API URL (default: http://localhost:5000)
    -SpaUrl <url>                  Frontend SPA URL (default: http://localhost:4200)
    -ConnectionString <connstr>    Custom PostgreSQL connection string
    -SkipDocker                    Skip Docker PostgreSQL setup (use existing PostgreSQL)
    -NoRun                         Setup only, don't start services
    -Help                          Show this help message

EXAMPLES:
    # Interactive setup with auto-generated JWT key
    .\setup.ps1

    # Non-interactive setup with all parameters
    .\setup.ps1 -j "your-64-char-key" -u "admin@example.com" -p "SecurePass123"

    # Setup without Docker, using custom PostgreSQL
    .\setup.ps1 -SkipDocker -ConnectionString "Host=mydb;Port=5432;Database=markitdb;Username=user;Password=pass"

    # Setup only, don't start services
    .\setup.ps1 -NoRun

"@
    exit 0
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"
    
    $allGood = $true
    
    # Check .NET SDK
    try {
        $dotnetVersion = & dotnet --version 2>&1
        if ($LASTEXITCODE -eq 0 -and $dotnetVersion -match "^([0-9]+)\.") {
            $majorVersion = [int]$Matches[1]
            if ($majorVersion -ge 9) {
                Write-Success ".NET SDK $dotnetVersion"
            } else {
                Write-Warning ".NET SDK $dotnetVersion found, but version 9.0+ recommended"
            }
        } else {
            throw
        }
    } catch {
        Write-Error ".NET SDK 9.0+ not found"
        Write-Info "Install from: https://dotnet.microsoft.com/download"
        $allGood = $false
    }
    
    # Check Node.js
    try {
        $nodeVersion = & node --version 2>&1
        if ($LASTEXITCODE -eq 0 -and $nodeVersion -match "v([0-9]+)\.") {
            $majorVersion = [int]$Matches[1]
            if ($majorVersion -ge 18) {
                Write-Success "Node.js $nodeVersion"
            } else {
                Write-Warning "Node.js $nodeVersion found, but version 18+ recommended"
            }
        } else {
            throw
        }
    } catch {
        Write-Error "Node.js 18+ not found"
        Write-Info "Install from: https://nodejs.org/"
        $allGood = $false
    }
    
    # Check pnpm
    try {
        $pnpmVersion = & pnpm --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "pnpm $pnpmVersion"
        } else {
            throw
        }
    } catch {
        Write-Error "pnpm not found"
        Write-Info "Install: npm install -g pnpm"
        $allGood = $false
    }
    
    # Check Docker (optional)
    if (-not $SkipDocker) {
        try {
            $dockerVersion = & docker --version 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Docker $dockerVersion"
            } else {
                throw
            }
        } catch {
            Write-Warning "Docker not found (optional)"
            Write-Info "Install from: https://www.docker.com/get-started"
            Write-Info "Or use -SkipDocker flag to use existing PostgreSQL"
        }
    }
    
    if (-not $allGood) {
        Write-Error "Missing required prerequisites. Please install them and try again."
        exit 1
    }
    
    Write-Success "All prerequisites satisfied!"
}

function New-JwtKey {
    $bytes = New-Object byte[] 64
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $base64 = [Convert]::ToBase64String($bytes)
    $key = $base64.Replace("=", "").Replace("+", "").Replace("/", "")
    
    if ($key.Length -lt 64) {
        $additionalBytes = New-Object byte[] (64 - $key.Length)
        $rng.GetBytes($additionalBytes)
        $key += [Convert]::ToBase64String($additionalBytes).Replace("=", "").Replace("+", "").Replace("/", "")
    }
    
    return $key.Substring(0, 64)
}

function Test-PasswordStrength {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingPlainTextForPassword', '', Justification='Internal validation function that processes password strings')]
    param([string]$Password)
    
    if ($Password.Length -lt 8) {
        return $false, "Password must be at least 8 characters long"
    }
    
    $hasUpper = $Password -cmatch "[A-Z]"
    $hasLower = $Password -cmatch "[a-z]"
    $hasDigit = $Password -match "[0-9]"
    
    if (-not ($hasUpper -and $hasLower -and $hasDigit)) {
        return $false, "Password must contain uppercase, lowercase, and digit"
    }
    
    return $true, ""
}

function Get-DockerConfig {
    Write-Host "PostgreSQL Docker Configuration:" -ForegroundColor Yellow
    Write-Host "  Customize database settings or press Enter to use defaults."
    Write-Host ""
    
    $defaults = @{
        DB_NAME = "markitdb"
        POSTGRES_USER = "postgres"
        POSTGRES_PASSWORD = "postgres"
        DB_PORT = "5432"
    }
    
    $config = @{
        DB_NAME = ""
        POSTGRES_USER = ""
        POSTGRES_PASSWORD = ""
        DB_PORT = ""
        UseCustom = $false
    }
    
    # Database Name
    do {
        $userInput = Read-Host "Database name (default: $($defaults.DB_NAME))"
        if ([string]::IsNullOrWhiteSpace($userInput)) {
            $config.DB_NAME = $defaults.DB_NAME
            break
        } elseif ($userInput -match '^[a-zA-Z0-9_-]+$') {
            $config.DB_NAME = $userInput
            $config.UseCustom = $true
            break
        } else {
            Write-Warning "Database name can only contain letters, numbers, underscores, and hyphens"
        }
    } while ($true)
    
    # PostgreSQL Server User
    $userInput = Read-Host "PostgreSQL server user (default: $($defaults.POSTGRES_USER))"
    if ([string]::IsNullOrWhiteSpace($userInput)) {
        $config.POSTGRES_USER = $defaults.POSTGRES_USER
    } else {
        $config.POSTGRES_USER = $userInput
        $config.UseCustom = $true
    }
    
    # PostgreSQL Server Password
    $securePass = Read-Host "PostgreSQL server password (press Enter for default: $($defaults.POSTGRES_PASSWORD))" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePass)
    $userInput = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    
    if ([string]::IsNullOrWhiteSpace($userInput)) {
        $config.POSTGRES_PASSWORD = $defaults.POSTGRES_PASSWORD
    } else {
        $config.POSTGRES_PASSWORD = $userInput
        $config.UseCustom = $true
    }
    
    # Database Port
    do {
        $userInput = Read-Host "Database port (default: $($defaults.DB_PORT))"
        if ([string]::IsNullOrWhiteSpace($userInput)) {
            $config.DB_PORT = $defaults.DB_PORT
            break
        } elseif ($userInput -match '^\d+$') {
            $port = [int]$userInput
            if ($port -ge 1 -and $port -le 65535) {
                $config.DB_PORT = $userInput
                $config.UseCustom = $true
                break
            } else {
                Write-Warning "Port must be between 1 and 65535"
            }
        } else {
            Write-Warning "Port must be a valid number"
        }
    } while ($true)
    
    Write-Host ""
    
    # Create .env file if custom values provided
    if ($config.UseCustom) {
        $envContent = @"
# Docker PostgreSQL Configuration
# Generated by setup.ps1
DB_NAME=$($config.DB_NAME)
POSTGRES_USER=$($config.POSTGRES_USER)
POSTGRES_PASSWORD=$($config.POSTGRES_PASSWORD)
DB_PORT=$($config.DB_PORT)
"@
        $envPath = Join-Path $SCRIPT_DIR ".env"
        Set-Content -Path $envPath -Value $envContent
        Write-Success "Docker configuration saved to .env"
    } else {
        Write-Info "Using default Docker configuration (no .env file created)"
    }
    
    return $config
}

function Remove-JsonComments {
    param([string]$JsonContent)
    
    $result = ""
    $inString = $false
    $i = 0
    
    while ($i -lt $JsonContent.Length) {
        $char = $JsonContent[$i]
        
        if ($char -eq '"' -and ($i -eq 0 -or $JsonContent[$i-1] -ne '\')) {
            $inString = -not $inString
            $result += $char
        }
        elseif (-not $inString -and $char -eq '/' -and ($i + 1) -lt $JsonContent.Length -and $JsonContent[$i + 1] -eq '/') {
            # Skip until end of line
            while ($i -lt $JsonContent.Length -and $JsonContent[$i] -ne "`n") { $i++ }
            if ($i -lt $JsonContent.Length) { $result += "`n" }
            continue
        }
        else {
            $result += $char
        }
        $i++
    }
    
    return $result
}

function Format-Json {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Json,
        [int]$Indentation = 2
    )
    
    # Format JSON with proper indentation (handles compressed JSON)
    $indent = 0
    $result = ""
    $inString = $false
    $escaped = $false
    
    for ($i = 0; $i -lt $Json.Length; $i++) {
        $char = $Json[$i]
        
        # Track if we're inside a string
        if ($char -eq '"' -and -not $escaped) {
            $inString = -not $inString
        }
        $escaped = ($char -eq '\' -and -not $escaped)
        
        # Handle structure characters outside strings
        if (-not $inString) {
            if ($char -eq '{' -or $char -eq '[') {
                $result += $char + "`n"
                $indent++
                $result += (' ' * ($indent * $Indentation))
            }
            elseif ($char -eq '}' -or $char -eq ']') {
                $result = $result.TrimEnd()
                $indent--
                $result += "`n" + (' ' * ($indent * $Indentation)) + $char
            }
            elseif ($char -eq ',') {
                $result += $char + "`n" + (' ' * ($indent * $Indentation))
            }
            elseif ($char -eq ':') {
                $result += $char + ' '
            }
            elseif ($char -match '\s') {
                # Skip whitespace outside strings
                continue
            }
            else {
                $result += $char
            }
        }
        else {
            # Inside strings, keep everything including whitespace
            $result += $char
        }
    }
    
    return $result.Trim()
}

function Get-UserInput {
    Write-Header "Configuration Setup"
    
    $config = @{
        JwtKey = $JwtKey
        AdminUser = $AdminUser
        AdminPassword = $AdminPassword
        ConnectionString = $ConnectionString
        DockerConfig = $null
    }
    
    # Docker Configuration (if not skipping Docker)
    if (-not $SkipDocker) {
        $config.DockerConfig = Get-DockerConfig
        
        # Auto-generate connection string from Docker config
        if (-not $config.ConnectionString) {
            $dbHost = "localhost"
            $dbPort = $config.DockerConfig.DB_PORT
            $dbName = $config.DockerConfig.DB_NAME
            $dbUser = $config.DockerConfig.POSTGRES_USER
            $dbPassword = $config.DockerConfig.POSTGRES_PASSWORD
            $config.ConnectionString = "Host=$dbHost;Port=$dbPort;Database=$dbName;Username=$dbUser;Password=$dbPassword"
            Write-Info "Connection string auto-generated from Docker configuration"
            Write-Host ""
        }
    } elseif (-not $config.ConnectionString) {
        $config.ConnectionString = $DEFAULT_CONNECTION_STRING
    }
    
    # JWT Key
    if (-not $config.JwtKey) {
        Write-Host "JWT Signing Key Configuration:" -ForegroundColor Yellow
        Write-Host "  A secure key is required to sign authentication tokens."
        Write-Host ""
        $choice = Read-Host "Do you want to auto-generate a secure JWT key? (Y/n)"
        
        if ($choice -eq "" -or $choice -eq "Y" -or $choice -eq "y") {
            $config.JwtKey = New-JwtKey
            Write-Success "JWT key auto-generated (64 characters)"
        } else {
            do {
                $config.JwtKey = Read-Host "Enter JWT key (minimum 32 characters)"
                if ($config.JwtKey.Length -lt 32) {
                    Write-Warning "JWT key must be at least 32 characters long"
                }
            } while ($config.JwtKey.Length -lt 32)
        }
        Write-Host ""
    }
    
    # Admin Username
    if (-not $config.AdminUser) {
        Write-Host "Admin Account Configuration:" -ForegroundColor Yellow
        $defaultUser = "admin@markit.local"
        $inputUser = Read-Host "Enter admin email (default: $defaultUser)"
        $config.AdminUser = if ($inputUser) { $inputUser } else { $defaultUser }
    }
    
    # Admin Password
    if (-not $config.AdminPassword) {
        do {
            $securePass = Read-Host "Enter admin password (min 8 chars, uppercase, lowercase, digit)" -AsSecureString
            $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePass)
            $config.AdminPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
            
            $isValid, $message = Test-PasswordStrength $config.AdminPassword
            if (-not $isValid) {
                Write-Warning $message
            }
        } while (-not $isValid)
    }
    
    # Connection String (if skipping Docker and not provided)
    if ($SkipDocker -and -not $ConnectionString) {
        Write-Host ""
        Write-Warning "Docker is disabled. Using default PostgreSQL connection string:"
        Write-Host "  $DEFAULT_CONNECTION_STRING" -ForegroundColor Gray
        Write-Host ""
        $customConn = Read-Host "Enter custom connection string (or press Enter to use default)"
        if ($customConn) {
            $config.ConnectionString = $customConn
        }
        Write-Host ""
    }
    
    return $config
}

function Initialize-Backend {
    param($Config)
    
    Write-Header "Setting Up Backend Configuration"
    
    if (Test-Path $APPSETTINGS_FILE) {
        Write-Warning "Backend configuration already exists: $APPSETTINGS_FILE"
        $overwrite = Read-Host "Overwrite existing configuration? (y/N)"
        if ($overwrite -ne "y" -and $overwrite -ne "Y") {
            Write-Info "Skipping backend configuration"
            return
        }
    }
    
    # Read template and remove JSON comments
    $templateContent = Get-Content $APPSETTINGS_TEMPLATE -Raw
    $cleanJson = Remove-JsonComments $templateContent
    $template = $cleanJson | ConvertFrom-Json
    
    # Update configuration
    $template.ConnectionStrings.ConnectionString = $Config.ConnectionString
    $template.SpaSettings.baseUrl = $SpaUrl
    $template.JwtSettings.Key = $Config.JwtKey
    $template.UserDefaultSettings.UserName = $Config.AdminUser
    $template.UserDefaultSettings.Password = $Config.AdminPassword
    
    # Save configuration with proper formatting
    $json = $template | ConvertTo-Json -Depth 10 -Compress
    $formattedJson = Format-Json $json
    
    # Save with UTF8 encoding (no BOM) for cross-platform compatibility
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($APPSETTINGS_FILE, $formattedJson, $utf8NoBom)
    
    Write-Success "Backend configuration created: $APPSETTINGS_FILE"
}

function Initialize-Frontend {
    Write-Header "Setting Up Frontend Configuration"
    
    if (Test-Path $ENV_FILE) {
        Write-Warning "Frontend configuration already exists: $ENV_FILE"
        $overwrite = Read-Host "Overwrite existing configuration? (y/N)"
        if ($overwrite -ne "y" -and $overwrite -ne "Y") {
            Write-Info "Skipping frontend configuration"
            return
        }
    }
    
    # Read template and replace values
    $envContent = Get-Content $ENV_TEMPLATE -Raw
    $envContent = $envContent.Replace('${production}', 'False')
    $envContent = $envContent.Replace('${baseApiUrl}', $ApiUrl)
    
    # Save configuration
    Set-Content -Path $ENV_FILE -Value $envContent
    
    Write-Success "Frontend configuration created: $ENV_FILE"
}

function Start-Database {
    param($DockerConfig)
    
    if ($SkipDocker) {
        Write-Header "Database Setup"
        Write-Warning "Docker is disabled. Ensure PostgreSQL is running and accessible."
        Write-Info "Connection string: $($Config.ConnectionString)"
        Write-Host ""
        return
    }
    
    Write-Header "Starting PostgreSQL Database"
    
    if ($DockerConfig) {
        Write-Info "Docker Configuration:"
        Write-Host "  Database: $($DockerConfig.DB_NAME)" -ForegroundColor Gray
        Write-Host "  User: $($DockerConfig.POSTGRES_USER)" -ForegroundColor Gray
        Write-Host "  Port: $($DockerConfig.DB_PORT)" -ForegroundColor Gray
        Write-Host ""
    }
    
    # Temporarily disable error stopping for docker commands
    $prevErrorPref = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    
    try {
        # Check if container exists and is running
        $containerStatus = & docker ps -a --filter "name=markit-postgres" --format "{{.Status}}" 2>&1
        
        if ($containerStatus -match "Up") {
            Write-Success "PostgreSQL container is already running"
            return
        }
        
        Write-Info "Starting PostgreSQL container (this may take a moment if pulling image)..."
        # Stop container and remove volume to ensure fresh initialization with environment variables
        Write-Info "Preparing fresh database initialization..."
        # These commands may fail if resources don't exist - that's OK
        $null = & docker-compose down 2>&1
        $null = & docker volume rm markit_postgres_data 2>&1
        
        # Create container with environment variables
        $dockerOutput = & docker-compose up -d postgres 2>&1
        $dockerExitCode = $LASTEXITCODE
        
        if ($dockerExitCode -eq 0) {
            Write-Success "PostgreSQL container started successfully"
            Write-Info "Waiting for PostgreSQL to be ready..."
            Start-Sleep -Seconds 5
        } else {
            Write-Error "docker-compose up failed with exit code: $dockerExitCode"
            if ($dockerOutput) {
                Write-Host "Docker output:" -ForegroundColor Gray
                $dockerOutput | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
            }
            Write-Info "Please ensure Docker is running: docker ps"
            Write-Info "You can try starting manually: docker-compose up -d postgres"
            exit 1
        }
    } finally {
        # Restore error preference
        $ErrorActionPreference = $prevErrorPref
    }
}

function Install-FrontendDependencies {
    Write-Header "Installing Frontend Dependencies"
    
    try {
        Push-Location $FRONTEND_DIR
        Write-Info "Running pnpm install..."
        & pnpm install --silent
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Frontend dependencies installed"
        } else {
            throw "pnpm install failed"
        }
    } catch {
        Write-Error "Failed to install frontend dependencies: $_"
        Write-Info "You can install manually with: cd Frontend && pnpm install"
    } finally {
        Pop-Location
    }
}

function Start-Services {
    Write-Header "Starting Services"
    
    Write-Host "Backend API will start at: $ApiUrl" -ForegroundColor Cyan
    Write-Host "Frontend will start at: $SpaUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "To start services manually later, use: .\scripts\run.ps1"
    Write-Host ""
    
    $startNow = Read-Host "Start services now? (Y/n)"
    
    if ($startNow -eq "" -or $startNow -eq "Y" -or $startNow -eq "y") {
        Write-Host ""
        Write-Info "Launching both services in split panes..."
        Write-Host ""
        
        $runScript = Join-Path $SCRIPT_DIR "scripts\run.ps1"
        & $runScript -All
    }
}

# Main Script
if ($Help) {
    Show-Help
    return
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "                                             " -ForegroundColor Cyan
Write-Host "             Markit Setup Script             " -ForegroundColor Cyan
Write-Host "                                             " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Run setup steps
Test-Prerequisites

# Check if already configured
$backendExists = Test-Path $APPSETTINGS_FILE
$frontendExists = Test-Path $ENV_FILE

if ($backendExists -and $frontendExists) {
    Write-Warning "Markit appears to be already configured."
    Write-Info "Configuration files found:"
    Write-Host "  - Backend: $APPSETTINGS_FILE" -ForegroundColor Gray
    Write-Host "  - Frontend: $ENV_FILE" -ForegroundColor Gray
    Write-Host ""
    $reconfigure = Read-Host "Do you want to reconfigure? (y/N)"
    if ($reconfigure -ne "y" -and $reconfigure -ne "Y") {
        Write-Info "Setup cancelled. Use .\scripts\run.ps1 to start services."
        exit 0
    }
}

$config = Get-UserInput
Initialize-Backend $config
Initialize-Frontend
Start-Database $config.DockerConfig
Install-FrontendDependencies

Write-Header "Setup Complete!"
Write-Success "Markit is ready!"
Write-Host ""
Write-Info "Configuration Summary:"
Write-Host "  Backend API: $ApiUrl" -ForegroundColor Gray
Write-Host "  Frontend: $SpaUrl" -ForegroundColor Gray
Write-Host "  Admin User: $($config.AdminUser)" -ForegroundColor Gray
$dbMode = if ($SkipDocker) { "(External)" } else { "(Docker)" }
Write-Host "  Database: PostgreSQL $dbMode" -ForegroundColor Gray

if (-not $NoRun) {
    Start-Services
}