#!/usr/bin/env pwsh
# Markit Run Script for Windows
# This script starts the Markit development environment

param(
    [switch]$Backend,
    [switch]$Frontend,
    [switch]$Docker,
    [switch]$All,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# Constants
$SCRIPT_DIR = Split-Path -Parent $PSScriptRoot
$BACKEND_DIR = Join-Path $SCRIPT_DIR "Backend\markitAPI\API"
$FRONTEND_DIR = Join-Path $SCRIPT_DIR "Frontend"
$APPSETTINGS_FILE = Join-Path $BACKEND_DIR "appsettings.json"
$ENV_FILE = Join-Path $FRONTEND_DIR "src\environments\environment.ts"

function Write-Info {
    param([string]$Text)
    Write-Host "[INFO] $Text" -ForegroundColor Cyan
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

function Show-Help {
    Write-Host @"

Markit Run Script - Start development services

USAGE:
    .\run.ps1 [OPTIONS]

OPTIONS:
    -Backend      Start only the backend API
    -Frontend     Start only the frontend
    -Docker       Start the PostgreSQL Docker container (if configured)
    -All          Start both backend and frontend in split panes (Windows Terminal) or separate windows
    -Help         Show this help message

EXAMPLES:
    # Start only backend
    .\run.ps1 -Backend

    # Start only frontend
    .\run.ps1 -Frontend

    # Start Docker database and backend
    .\run.ps1 -Docker -Backend

    # Start both backend and frontend together
    .\run.ps1 -All

    # Start Docker and both services
    .\run.ps1 -Docker -All

"@
    exit 0
}

function Test-Configuration {
    $configured = $true
    
    if (-not (Test-Path $APPSETTINGS_FILE)) {
        Write-Error "Backend configuration not found: $APPSETTINGS_FILE"
        $configured = $false
    }
    
    if (-not (Test-Path $ENV_FILE)) {
        Write-Error "Frontend configuration not found: $ENV_FILE"
        $configured = $false
    }
    
    if (-not $configured) {
        Write-Info "Please run setup first: .\setup.ps1"
        exit 1
    }
}

function Test-WindowsTerminal {
    try {
        $null = Get-Command wt -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

function Start-BothServicesInSeparateWindows {
    Write-Info "Starting Backend in new window..."
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$BACKEND_DIR'; Write-Host 'Starting Backend API...' -ForegroundColor Cyan; Write-Host ''; dotnet run --launch-profile Development"
    
    Start-Sleep -Milliseconds 500
    
    Write-Info "Starting Frontend in new window..."
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$FRONTEND_DIR'; Write-Host 'Starting Frontend...' -ForegroundColor Cyan; Write-Host ''; pnpm start"
    
    Write-Host ""
    Write-Success "Services started in separate windows"
    Write-Host ""
    Write-Host "Services will be available at:" -ForegroundColor Yellow
    Write-Host "  - Backend API: http://localhost:5000" -ForegroundColor Cyan
    Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host "  - Frontend: http://localhost:4200" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "Close each window to stop the corresponding service"
    Write-Host ""
}

function Start-BothServicesInSplitPane {
    if (Test-WindowsTerminal) {
        Write-Info "Detected Windows Terminal - launching services in split panes..."
        Write-Host ""
        
        try {
            # Create temporary script files in the project directory (more reliable than TEMP)
            $tempDir = Join-Path $SCRIPT_DIR ".markit-temp"
            if (-not (Test-Path $tempDir)) {
                New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
            }
            
            $backendBat = Join-Path $tempDir "backend.bat"
            $frontendBat = Join-Path $tempDir "frontend.bat"
            
            # Backend batch script
            @"
@echo off
cd /d "$BACKEND_DIR"
echo Starting Backend API...
echo.
echo Backend will be available at:
echo   - API: http://localhost:5000
echo   - Swagger: http://localhost:5000/swagger
echo.
dotnet run --launch-profile Development
"@ | Out-File -FilePath $backendBat -Encoding ASCII -Force
            
            # Frontend batch script
            @"
@echo off
cd /d "$FRONTEND_DIR"
echo Starting Frontend...
echo.
echo Frontend will be available at:
echo   - http://localhost:4200
echo.
pnpm start
"@ | Out-File -FilePath $frontendBat -Encoding ASCII -Force
            
            # Verify files were created
            if (-not (Test-Path $backendBat)) {
                throw "Failed to create backend script file at: $backendBat"
            }
            if (-not (Test-Path $frontendBat)) {
                throw "Failed to create frontend script file at: $frontendBat"
            }
            
            # Launch Windows Terminal with split panes
            # Using batch files avoids PowerShell quoting/escaping issues
            # -V creates a vertical split (side-by-side panes)
            & wt.exe new-tab --title "Backend API" cmd.exe /k "$backendBat" `;  split-pane -V --title "Frontend" cmd.exe /k "$frontendBat"
            
            Write-Success "Services launched in Windows Terminal split panes"
            Write-Host ""
            Write-Host "Services will be available at:" -ForegroundColor Yellow
            Write-Host "  - Backend API: http://localhost:5000" -ForegroundColor Cyan
            Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
            Write-Host "  - Frontend: http://localhost:4200" -ForegroundColor Cyan
            Write-Host ""
            Write-Info "A new Windows Terminal tab with two panes has been created"
            Write-Info "Press Ctrl+C in each pane to stop the corresponding service"
            Write-Host ""
        } catch {
            Write-Warning "Failed to launch Windows Terminal split panes: $_"
            Write-Info "Falling back to separate windows..."
            Start-BothServicesInSeparateWindows
        }
    } else {
        Write-Info "Windows Terminal not detected - using separate windows..."
        Write-Host ""
        Start-BothServicesInSeparateWindows
    }
}

function Start-Database {
    Write-Info "Starting PostgreSQL Docker container..."
    
    # Check if Docker is available
    try {
        $null = & docker --version 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Docker not available"
        }
    } catch {
        Write-Warning "Docker not available. Ensure PostgreSQL is running manually."
        return
    }
    
    # Check if container exists
    $containerExists = & docker ps -a --filter "name=markit-postgres" --format "{{.Names}}" 2>&1
    
    if (-not $containerExists -or $containerExists -notmatch "markit-postgres") {
        Write-Error "PostgreSQL container not found."
        Write-Info "Please run the setup script first: .\setup.ps1"
        exit 1
    }
    
    # Check if container is running
    $containerStatus = & docker ps --filter "name=markit-postgres" --format "{{.Status}}" 2>&1
    
    if ($containerStatus -match "Up") {
        Write-Success "PostgreSQL container is already running"
    } else {
        Write-Info "Starting existing PostgreSQL container..."
        & docker start markit-postgres 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "PostgreSQL container started"
        } else {
            Write-Error "Failed to start PostgreSQL container."
            Write-Info "Ensure Docker is running and the container is properly configured."
            exit 1
        }
    }
}

function Start-Backend {
    Write-Info "Starting Backend API..."
    Write-Host ""
    Write-Host "Backend will be available at:" -ForegroundColor Yellow
    Write-Host "  - API: http://localhost:5000" -ForegroundColor Cyan
    Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "Press Ctrl+C to stop the backend"
    Write-Host ""
    
    Push-Location $BACKEND_DIR
    try {
        & dotnet run --launch-profile "Development"
    } finally {
        Pop-Location
    }
}

function Start-Frontend {
    Write-Info "Starting Frontend..."
    Write-Host ""
    Write-Host "Frontend will be available at:" -ForegroundColor Yellow
    Write-Host "  - http://localhost:4200" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "Press Ctrl+C to stop the frontend"
    Write-Host ""
    
    Push-Location $FRONTEND_DIR
    try {
        & pnpm start
    } finally {
        Pop-Location
    }
}

function Start-AllServices {
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "                                          " -ForegroundColor Cyan
    Write-Host "      Starting Markit Services            " -ForegroundColor Cyan
    Write-Host "                                          " -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Choose which service to run:" -ForegroundColor Yellow
    Write-Host "  1. Backend API (http://localhost:5000)" -ForegroundColor Gray
    Write-Host "  2. Frontend (http://localhost:4200)" -ForegroundColor Gray
    Write-Host "  3. Both (recommended: use separate terminals)" -ForegroundColor Gray
    Write-Host ""
    
    $choice = Read-Host "Enter your choice (1-3)"
    
    switch ($choice) {
        "1" {
            Start-Backend
        }
        "2" {
            Start-Frontend
        }
        "3" {
            Start-BothServicesInSplitPane
        }
        default {
            Write-Error "Invalid choice"
            exit 1
        }
    }
}

# Main Script
if ($Help) {
    Show-Help
}

Test-Configuration

# Validate flag conflicts
if ($All -and ($Backend -or $Frontend)) {
    Write-Error "Cannot use -All with -Backend or -Frontend flags"
    Write-Info "Use either:"
    Write-Host "  .\run.ps1 -All" -ForegroundColor Yellow
    Write-Host "  .\run.ps1 -Backend" -ForegroundColor Yellow
    Write-Host "  .\run.ps1 -Frontend" -ForegroundColor Yellow
    exit 1
}

# Determine what to run
$runDocker = $Docker
$runBackend = $Backend
$runFrontend = $Frontend
$runAll = $All

if ($runAll) {
    # Run both services in split panes or separate windows
    if ($runDocker) {
        Start-Database
        Write-Host ""
    }
    Start-BothServicesInSplitPane
} elseif (-not ($Backend -or $Frontend -or $Docker -or $All)) {
    # No specific service selected, run interactive mode
    Start-AllServices
} else {
    # Specific services selected
    if ($runDocker) {
        Start-Database
    }
    
    if ($runBackend -and $runFrontend) {
        Write-Warning "Cannot run both Backend and Frontend in the same terminal"
        Write-Info "Please use separate terminals or the -All flag:"
        Write-Host "  Terminal 1: .\run.ps1 -Backend" -ForegroundColor Yellow
        Write-Host "  Terminal 2: .\run.ps1 -Frontend" -ForegroundColor Yellow
        Write-Host "  Or: .\run.ps1 -All" -ForegroundColor Green
        exit 1
    } elseif ($runBackend) {
        Start-Backend
    } elseif ($runFrontend) {
        Start-Frontend
    }
}
