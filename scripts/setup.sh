#!/bin/bash
# Markit Setup Script for Linux/Mac
# This script automates the setup of Markit development environment

set -e

# Parse command line arguments
JWT_KEY=""
ADMIN_USER=""
ADMIN_PASSWORD=""
API_URL="http://localhost:5000"
SPA_URL="http://localhost:4200"
CONNECTION_STRING=""
SKIP_DOCKER=false
NO_RUN=false
SHOW_HELP=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -j|--jwt-key)
            JWT_KEY="$2"
            shift 2
            ;;
        -u|--admin-user)
            ADMIN_USER="$2"
            shift 2
            ;;
        -p|--admin-password)
            ADMIN_PASSWORD="$2"
            shift 2
            ;;
        --api-url)
            API_URL="$2"
            shift 2
            ;;
        --spa-url)
            SPA_URL="$2"
            shift 2
            ;;
        --connection-string)
            CONNECTION_STRING="$2"
            shift 2
            ;;
        --skip-docker)
            SKIP_DOCKER=true
            shift
            ;;
        --no-run)
            NO_RUN=true
            shift
            ;;
        --help)
            SHOW_HELP=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Constants
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKEND_DIR="$SCRIPT_DIR/Backend/markitAPI/API"
FRONTEND_DIR="$SCRIPT_DIR/Frontend"
APPSETTINGS_TEMPLATE="$BACKEND_DIR/appsettings-template.json"
APPSETTINGS_FILE="$BACKEND_DIR/appsettings.json"
ENV_TEMPLATE="$FRONTEND_DIR/src/environments/environment.template.ts"
ENV_FILE="$FRONTEND_DIR/src/environments/environment.ts"
DEFAULT_CONNECTION_STRING="Host=localhost;Port=5432;Database=markitdb;Username=postgres;Password=postgres"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

# Helper Functions
write_header() {
    echo ""
    echo -e "${CYAN}=============================================${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}=============================================${NC}"
    echo ""
}

write_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

write_error() {
    echo -e "${RED}✗ $1${NC}"
}

write_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

write_info() {
    echo -e "${CYAN}ℹ $1${NC}"
}

remove_json_comments() {
    # Remove JSON comments (// ...) from input
    # Reads from file path passed as argument
    local file="$1"
    grep -v '^\s*//' "$file" | sed 's|^\([^"]*\("[^"]*"[^"]*\)*\)//.*|\1|'
}

show_help() {
    cat << EOF

Markit Setup Script - Automates development environment setup

USAGE:
    ./setup.sh [OPTIONS]

OPTIONS:
    -j, --jwt-key <key>              JWT signing key (64+ characters, auto-generated if not provided)
    -u, --admin-user <email>         Admin username/email (default: admin@markit.local)
    -p, --admin-password <password>  Admin password (required, must be strong)
    --api-url <url>                  Backend API URL (default: http://localhost:5000)
    --spa-url <url>                  Frontend SPA URL (default: http://localhost:4200)
    --connection-string <connstr>    Custom PostgreSQL connection string
    --skip-docker                    Skip Docker PostgreSQL setup (use existing PostgreSQL)
    --no-run                         Setup only, don't start services
    --help                           Show this help message

EXAMPLES:
    # Interactive setup with auto-generated JWT key
    ./setup.sh

    # Non-interactive setup with all parameters
    ./setup.sh -j "your-64-char-key" -u "admin@example.com" -p "SecurePass123"

    # Setup without Docker, using custom PostgreSQL
    ./setup.sh --skip-docker --connection-string "Host=mydb;Port=5432;Database=markitdb;Username=user;Password=pass"

    # Setup only, don't start services
    ./setup.sh --no-run

EOF
    exit 0
}

test_prerequisites() {
    write_header "Checking Prerequisites"
    
    local all_good=true
    
    # Check .NET SDK
    if command -v dotnet &> /dev/null; then
        local dotnet_version=$(dotnet --version 2>&1)
        local major_version=$(echo "$dotnet_version" | cut -d'.' -f1)
        if [[ $major_version -ge 9 ]]; then
            write_success ".NET SDK $dotnet_version"
        else
            write_warning ".NET SDK $dotnet_version found, but version 9.0+ recommended"
        fi
    else
        write_error ".NET SDK 9.0+ not found"
        write_info "Install from: https://dotnet.microsoft.com/download"
        all_good=false
    fi
    
    # Check Node.js
    if command -v node &> /dev/null; then
        local node_version=$(node --version 2>&1)
        local major_version=$(echo "$node_version" | sed 's/v\([0-9]*\).*/\1/')
        if [[ $major_version -ge 18 ]]; then
            write_success "Node.js $node_version"
        else
            write_warning "Node.js $node_version found, but version 18+ recommended"
        fi
    else
        write_error "Node.js 18+ not found"
        write_info "Install from: https://nodejs.org/"
        all_good=false
    fi
    
    # Check pnpm
    if command -v pnpm &> /dev/null; then
        local pnpm_version=$(pnpm --version 2>&1)
        write_success "pnpm $pnpm_version"
    else
        write_error "pnpm not found"
        write_info "Install: npm install -g pnpm"
        all_good=false
    fi
    
    # Check Docker (optional)
    if [[ "$SKIP_DOCKER" == false ]]; then
        if command -v docker &> /dev/null; then
            local docker_version=$(docker --version 2>&1)
            write_success "Docker $docker_version"
        else
            write_warning "Docker not found (optional)"
            write_info "Install from: https://www.docker.com/get-started"
            write_info "Or use --skip-docker flag to use existing PostgreSQL"
        fi
    fi
    
    if [[ "$all_good" == false ]]; then
        write_error "Missing required prerequisites. Please install them and try again."
        exit 1
    fi
    
    write_success "All prerequisites satisfied!"
}

generate_jwt_key() {
    local key=$(openssl rand -base64 48 | tr -d "=+/" | tr -d '\n' | cut -c1-64)
    echo "$key"
}

test_password_strength() {
    local password="$1"
    
    if [[ ${#password} -lt 8 ]]; then
        echo "Password must be at least 8 characters long"
        return 1
    fi
    
    if ! [[ "$password" =~ [A-Z] ]]; then
        echo "Password must contain at least one uppercase letter"
        return 1
    fi
    
    if ! [[ "$password" =~ [a-z] ]]; then
        echo "Password must contain at least one lowercase letter"
        return 1
    fi
    
    if ! [[ "$password" =~ [0-9] ]]; then
        echo "Password must contain at least one digit"
        return 1
    fi
    
    return 0
}

get_docker_config() {
    echo -e "${YELLOW}PostgreSQL Docker Configuration:${NC}"
    echo "  Customize database settings or press Enter to use defaults."
    echo ""
    
    local defaults_db_name="markitdb"
    local defaults_postgres_user="postgres"
    local defaults_postgres_password="postgres"
    local defaults_db_port="5432"
    
    DB_NAME=""
    POSTGRES_USER=""
    POSTGRES_PASSWORD=""
    DB_PORT=""
    local use_custom=false
    
    # Database Name
    while true; do
        read -p "Database name (default: $defaults_db_name): " input
        if [[ -z "$input" ]]; then
            DB_NAME="$defaults_db_name"
            break
        elif [[ "$input" =~ ^[a-zA-Z0-9_-]+$ ]]; then
            DB_NAME="$input"
            use_custom=true
            break
        else
            write_warning "Database name can only contain letters, numbers, underscores, and hyphens"
        fi
    done
    
    # PostgreSQL Server User
    read -p "PostgreSQL server user (default: $defaults_postgres_user): " input
    if [[ -z "$input" ]]; then
        POSTGRES_USER="$defaults_postgres_user"
    else
        POSTGRES_USER="$input"
        use_custom=true
    fi
    
    # PostgreSQL Server Password
    read -p "PostgreSQL server password (default: $defaults_postgres_password): " input
    if [[ -z "$input" ]]; then
        POSTGRES_PASSWORD="$defaults_postgres_password"
    else
        POSTGRES_PASSWORD="$input"
        use_custom=true
    fi
    
    # Database Port
    while true; do
        read -p "Database port (default: $defaults_db_port): " input
        if [[ -z "$input" ]]; then
            DB_PORT="$defaults_db_port"
            break
        elif [[ "$input" =~ ^[0-9]+$ ]] && [[ "$input" -ge 1 ]] && [[ "$input" -le 65535 ]]; then
            DB_PORT="$input"
            use_custom=true
            break
        else
            write_warning "Port must be a valid number between 1 and 65535"
        fi
    done
    
    echo ""
    
    # Create .env file if custom values provided
    if [[ "$use_custom" == true ]]; then
        cat > "$SCRIPT_DIR/.env" << EOF
# Docker PostgreSQL Configuration
# Generated by setup.sh
DB_NAME=$DB_NAME
POSTGRES_USER=$POSTGRES_USER
POSTGRES_PASSWORD=$POSTGRES_PASSWORD
DB_PORT=$DB_PORT
EOF
        write_success "Docker configuration saved to .env"
    else
        write_info "Using default Docker configuration (no .env file created)"
    fi
}

get_user_input() {
    write_header "Configuration Setup"
    
    # Docker Configuration (if not skipping Docker)
    if [[ "$SKIP_DOCKER" == false ]]; then
        get_docker_config
        
        # Auto-generate connection string from Docker config
        if [[ -z "$CONNECTION_STRING" ]]; then
            CONNECTION_STRING="Host=localhost;Port=$DB_PORT;Database=$DB_NAME;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD"
            write_info "Connection string auto-generated from Docker configuration"
            echo ""
        fi
    fi
    
    # JWT Key
    if [[ -z "$JWT_KEY" ]]; then
        echo -e "${YELLOW}JWT Signing Key Configuration:${NC}"
        echo "  A secure key is required to sign authentication tokens."
        echo ""
        read -p "Do you want to auto-generate a secure JWT key? (Y/n): " choice
        
        if [[ -z "$choice" || "$choice" == "Y" || "$choice" == "y" ]]; then
            JWT_KEY=$(generate_jwt_key)
            write_success "JWT key auto-generated (64 characters)"
        else
            while true; do
                read -p "Enter JWT key (minimum 32 characters): " JWT_KEY
                if [[ ${#JWT_KEY} -ge 32 ]]; then
                    break
                fi
                write_warning "JWT key must be at least 32 characters long"
            done
        fi
        echo ""
    fi
    
    # Admin Username
    if [[ -z "$ADMIN_USER" ]]; then
        echo -e "${YELLOW}Admin Account Configuration:${NC}"
        local default_user="admin@markit.local"
        read -p "Enter admin email (default: $default_user): " input_user
        ADMIN_USER=${input_user:-$default_user}
    fi
    
    # Admin Password
    if [[ -z "$ADMIN_PASSWORD" ]]; then
        while true; do
            read -sp "Enter admin password (min 8 chars, uppercase, lowercase, digit): " ADMIN_PASSWORD
            echo ""
            
            if error_msg=$(test_password_strength "$ADMIN_PASSWORD"); then
                break
            else
                write_warning "$error_msg"
            fi
        done
    fi
    
    # Connection String (if skipping Docker and not provided)
    if [[ "$SKIP_DOCKER" == true && -z "$CONNECTION_STRING" ]]; then
        echo ""
        write_warning "Docker is disabled. Using default PostgreSQL connection string:"
        echo -e "  ${GRAY}$DEFAULT_CONNECTION_STRING${NC}"
        echo ""
        read -p "Enter custom connection string (or press Enter to use default): " custom_conn
        if [[ -n "$custom_conn" ]]; then
            CONNECTION_STRING="$custom_conn"
        else
            CONNECTION_STRING="$DEFAULT_CONNECTION_STRING"
        fi
        echo ""
    fi
}

setup_backend() {
    write_header "Setting Up Backend Configuration"
    
    if [[ -f "$APPSETTINGS_FILE" ]]; then
        write_warning "Backend configuration already exists: $APPSETTINGS_FILE"
        read -p "Overwrite existing configuration? (y/N): " overwrite
        if [[ "$overwrite" != "y" && "$overwrite" != "Y" ]]; then
            write_info "Skipping backend configuration"
            return
        fi
    fi
    
    # Read template and update values using jq or sed
    # First, remove JSON comments from the template
    TEMP_JSON=$(mktemp)
    remove_json_comments "$APPSETTINGS_TEMPLATE" > "$TEMP_JSON"
    
    if command -v jq &> /dev/null; then
        # Use jq for JSON manipulation (preferred)
        jq --arg connStr "$CONNECTION_STRING" \
           --arg spaUrl "$SPA_URL" \
           --arg jwtKey "$JWT_KEY" \
           --arg adminUser "$ADMIN_USER" \
           --arg adminPass "$ADMIN_PASSWORD" \
           '.ConnectionStrings.ConnectionString = $connStr |
            .SpaSettings.baseUrl = $spaUrl |
            .JwtSettings.Key = $jwtKey |
            .UserDefaultSettings.UserName = $adminUser |
            .UserDefaultSettings.Password = $adminPass' \
           "$TEMP_JSON" > "$APPSETTINGS_FILE"
        rm "$TEMP_JSON"
    else
        # Fallback: manual sed replacements
        cp "$TEMP_JSON" "$APPSETTINGS_FILE"
        rm "$TEMP_JSON"
        
        # Escape special characters for sed (backslashes, slashes, ampersands) and remove newlines
        CONNECTION_STRING_ESC=$(printf '%s' "$CONNECTION_STRING" | sed 's/\\/\\\\/g; s/[\/&]/\\&/g' | tr -d '\n')
        SPA_URL_ESC=$(printf '%s' "$SPA_URL" | sed 's/\\/\\\\/g; s/[\/&]/\\&/g' | tr -d '\n')
        JWT_KEY_ESC=$(printf '%s' "$JWT_KEY" | sed 's/\\/\\\\/g; s/[\/&]/\\&/g' | tr -d '\n')
        ADMIN_USER_ESC=$(printf '%s' "$ADMIN_USER" | sed 's/\\/\\\\/g; s/[\/&]/\\&/g' | tr -d '\n')
        ADMIN_PASSWORD_ESC=$(printf '%s' "$ADMIN_PASSWORD" | sed 's/\\/\\\\/g; s/[\/&]/\\&/g' | tr -d '\n')
        
        sed -i.bak "s/\"ConnectionString\": \"\"/\"ConnectionString\": \"$CONNECTION_STRING_ESC\"/" "$APPSETTINGS_FILE"
        sed -i.bak "s/\"baseUrl\": \"\"/\"baseUrl\": \"$SPA_URL_ESC\"/" "$APPSETTINGS_FILE"
        sed -i.bak "s/\"Key\": \"\"/\"Key\": \"$JWT_KEY_ESC\"/" "$APPSETTINGS_FILE"
        sed -i.bak "s/\"UserName\": \"\"/\"UserName\": \"$ADMIN_USER_ESC\"/" "$APPSETTINGS_FILE"
        sed -i.bak "s/\"Password\": \"\"/\"Password\": \"$ADMIN_PASSWORD_ESC\"/" "$APPSETTINGS_FILE"
        
        rm -f "$APPSETTINGS_FILE.bak"
    fi
    
    write_success "Backend configuration created: $APPSETTINGS_FILE"
}

setup_frontend() {
    write_header "Setting Up Frontend Configuration"
    
    if [[ -f "$ENV_FILE" ]]; then
        write_warning "Frontend configuration already exists: $ENV_FILE"
        read -p "Overwrite existing configuration? (y/N): " overwrite
        if [[ "$overwrite" != "y" && "$overwrite" != "Y" ]]; then
            write_info "Skipping frontend configuration"
            return
        fi
    fi
    
    # Read template and replace values
    sed "s/\${production}/False/g; s|\${baseApiUrl}|$API_URL|g" "$ENV_TEMPLATE" > "$ENV_FILE"
    
    write_success "Frontend configuration created: $ENV_FILE"
}

start_database() {
    if [[ "$SKIP_DOCKER" == true ]]; then
        write_header "Database Setup"
        write_warning "Docker is disabled. Ensure PostgreSQL is running and accessible."
        write_info "Connection string: $CONNECTION_STRING"
        echo ""
        return
    fi
    
    write_header "Starting PostgreSQL Database"
    
    if [[ -n "$DB_NAME" ]]; then
        write_info "Docker Configuration:"
        echo -e "  ${GRAY}Database: $DB_NAME${NC}"
        echo -e "  ${GRAY}User: $POSTGRES_USER${NC}"
        echo -e "  ${GRAY}Port: $DB_PORT${NC}"
        echo ""
    fi
    
    # Check if container exists and is running
    if docker ps --filter "name=markit-postgres" --format "{{.Status}}" 2>/dev/null | grep -q "Up"; then
        write_success "PostgreSQL container is already running"
    else
        write_info "Starting PostgreSQL container (this may take a moment if pulling image)..."
        # Stop container and remove volume to ensure fresh initialization with environment variables
        write_info "Preparing fresh database initialization..."
        
        # These commands may fail if resources don't exist - that's OK
        docker-compose down > /dev/null 2>&1 || true
        docker volume rm markit_postgres_data > /dev/null 2>&1 || true
        
        # Create container with environment variables
        docker_output=$(docker-compose up -d postgres 2>&1)
        docker_exit_code=$?
        
        if [[ $docker_exit_code -ne 0 ]]; then
            echo ""
            write_error "Failed to start PostgreSQL container (exit code: $docker_exit_code)"
            if [[ -n "$docker_output" ]]; then
                write_info "Docker output:"
                echo "$docker_output" | while IFS= read -r line; do
                    echo "  $line"
                done
            fi
            write_info "Please ensure Docker is running: docker ps"
            write_info "You can try starting manually: docker-compose up -d postgres"
            exit 1
        fi
        
        write_success "PostgreSQL container started successfully"
        write_info "Waiting for PostgreSQL to be ready..."
        sleep 5
    fi
}

install_frontend_dependencies() {
    write_header "Installing Frontend Dependencies"
    
    cd "$FRONTEND_DIR"
    write_info "Running pnpm install..."
    pnpm install --silent
    
    if [[ $? -eq 0 ]]; then
        write_success "Frontend dependencies installed"
    else
        write_error "Failed to install frontend dependencies"
        write_info "You can install manually with: cd Frontend && pnpm install"
    fi
    cd "$SCRIPT_DIR"
}

start_services() {
    write_header "Starting Services"
    
    echo -e "${CYAN}Backend API will start at: $API_URL${NC}"
    echo -e "${CYAN}Frontend will start at: $SPA_URL${NC}"
    echo ""
    write_info "To start services manually later, use: ./scripts/run.sh"
    echo ""
    
    read -p "Start services now? (Y/n): " start_now
    
    if [[ -z "$start_now" || "$start_now" == "Y" || "$start_now" == "y" ]]; then
        echo ""
        write_info "Launching both services in split panes..."
        echo ""
        
        "$SCRIPT_DIR/scripts/run.sh" --all
    fi
}

# Main Script
if [[ "$SHOW_HELP" == true ]]; then
    show_help
fi

echo ""
echo -e "${CYAN}╔═══════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                                           ║${NC}"
echo -e "${CYAN}║            Markit Setup Script            ║${NC}"
echo -e "${CYAN}║                                           ║${NC}"
echo -e "${CYAN}╚═══════════════════════════════════════════╝${NC}"
echo ""

# Run setup steps
test_prerequisites

# Check if already configured
if [[ -f "$APPSETTINGS_FILE" && -f "$ENV_FILE" ]]; then
    write_warning "Markit appears to be already configured."
    write_info "Configuration files found:"
    echo -e "  ${GRAY}- Backend: $APPSETTINGS_FILE${NC}"
    echo -e "  ${GRAY}- Frontend: $ENV_FILE${NC}"
    echo ""
    read -p "Do you want to reconfigure? (y/N): " reconfigure
    if [[ "$reconfigure" != "y" && "$reconfigure" != "Y" ]]; then
        write_info "Setup cancelled. Use ./run.sh to start services."
        exit 0
    fi
fi

get_user_input
setup_backend
setup_frontend
start_database
install_frontend_dependencies

write_header "Setup Complete!"
write_success "Markit is ready for development!"
echo ""
write_info "Configuration Summary:"
echo -e "  ${GRAY}Backend API: $API_URL${NC}"
echo -e "  ${GRAY}Frontend: $SPA_URL${NC}"
echo -e "  ${GRAY}Admin User: $ADMIN_USER${NC}"
if [[ "$SKIP_DOCKER" == true ]]; then
    echo -e "  ${GRAY}Database: PostgreSQL (External)${NC}"
else
    echo -e "  ${GRAY}Database: PostgreSQL (Docker)${NC}"
fi

if [[ "$NO_RUN" == false ]]; then
    start_services
fi