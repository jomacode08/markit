#!/bin/bash
# Markit Run Script for Linux/Mac
# This script starts the Markit development environment

set -e

# Parse command line arguments
RUN_BACKEND=false
RUN_FRONTEND=false
RUN_DOCKER=false
RUN_ALL=false
SHOW_HELP=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --backend)
            RUN_BACKEND=true
            shift
            gnome-terminal --tab -- bash -c "cd '$FRONTEND_DIR' && echo -e '\033[0;36mStarting Frontend...\033[0m' && echo '' && pnpm start; exec bash" 2>/dev/null        --frontend)
            RUN_FRONTEND=true
            shift
            ;;
        --docker)
            RUN_DOCKER=true
            shift
            ;;
        --all)
            RUN_ALL=true
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
APPSETTINGS_FILE="$BACKEND_DIR/appsettings.json"
ENV_FILE="$FRONTEND_DIR/src/environments/environment.ts"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

write_info() {
    echo -e "${CYAN}ℹ $1${NC}"
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

show_help() {
    cat << EOF

Markit Run Script - Start development services

USAGE:
    ./run.sh [OPTIONS]

OPTIONS:
    --backend      Start only the backend API
    --frontend     Start only the frontend
    --docker       Start the PostgreSQL Docker container (if configured)
    --all          Start both backend and frontend in split panes/tabs or separate windows
    --help         Show this help message

EXAMPLES:
    # Start only backend
    ./run.sh --backend

    # Start only frontend
    ./run.sh --frontend

    # Start Docker database and backend
    ./run.sh --docker --backend

    # Start both backend and frontend together
    ./run.sh --all

    # Start Docker and both services
    ./run.sh --docker --all

EOF
    exit 0
}

test_configuration() {
    local configured=true
    
    if [[ ! -f "$APPSETTINGS_FILE" ]]; then
        write_error "Backend configuration not found: $APPSETTINGS_FILE"
        configured=false
    fi
    
    if [[ ! -f "$ENV_FILE" ]]; then
        write_error "Frontend configuration not found: $ENV_FILE"
        configured=false
    fi
    
    if [[ "$configured" == false ]]; then
        write_info "Please run setup first: ./setup.sh"
        exit 1
    fi
}

detect_terminal() {
    # Check for Windows Terminal (WSL scenarios)
    if command -v wt.exe &> /dev/null; then
        echo "windows-terminal"
        return
    fi
    
    # Check for gnome-terminal (Linux)
    if command -v gnome-terminal &> /dev/null; then
        echo "gnome"
        return
    fi
    
    # Check for macOS Terminal
    if [[ "$OSTYPE" == "darwin"* ]] && command -v osascript &> /dev/null; then
        echo "macos"
        return
    fi
    
    # Check for tmux
    if command -v tmux &> /dev/null; then
        echo "tmux"
        return
    fi
    
    # No supported terminal found
    echo "none"
}

start_both_services_separate_windows() {
    local terminal_type=$(detect_terminal)
    
    case $terminal_type in
        gnome)
            write_info "Starting Backend in new tab..."
            gnome-terminal --tab -- bash -c "cd '$BACKEND_DIR' && echo -e '\033[0;36mStarting Backend API...\033[0m' && echo '' && dotnet run --launch-profile Development; exec bash" 2>/dev/null
            
            sleep 0.5
            
            write_info "Starting Frontend in new tab..."
            gnome-terminal --tab -- bash -c "cd '$FRONTEND_DIR' && echo -e '\033[0;36mStarting Frontend...\033[0m' && echo '' && pnpm start; exec bash" 2>/dev/null            
            ;;
        macos)
            write_info "Starting Backend in new terminal window..."
            osascript -e "tell application \"Terminal\" to do script \"cd '$BACKEND_DIR' && echo 'Starting Backend API...' && echo '' && dotnet run --launch-profile Development\"" &>/dev/null
            
            sleep 0.5
            
            write_info "Starting Frontend in new terminal window..."
            osascript -e "tell application \"Terminal\" to do script \"cd '$FRONTEND_DIR' && echo 'Starting Frontend...' && echo '' && pnpm start\"" &>/dev/null
            ;;
        *)
            # Fallback: try xterm or provide manual instructions
            if command -v xterm &> /dev/null; then
                write_info "Starting Backend in new xterm window..."
                xterm -e "bash -c 'cd \"$BACKEND_DIR\" && echo \"Starting Backend API...\" && echo && dotnet run --launch-profile Development; exec bash'" &
                
                sleep 0.5
                
                write_info "Starting Frontend in new xterm window..."
                xterm -e "bash -c 'cd \"$FRONTEND_DIR\" && echo \"Starting Frontend...\" && echo && pnpm start; exec bash'" &
            else
                write_warning "Could not detect a supported terminal emulator"
                write_info "Please open two separate terminals and run:"
                echo -e "  ${YELLOW}Terminal 1: ./run.sh --backend${NC}"
                echo -e "  ${YELLOW}Terminal 2: ./run.sh --frontend${NC}"
                exit 1
            fi
            ;;
    esac
    
    echo ""
    write_success "Services started in separate windows/tabs"
    echo ""
    echo -e "${YELLOW}Services will be available at:${NC}"
    echo -e "${CYAN}  - Backend API: http://localhost:5000${NC}"
    echo -e "${CYAN}  - Swagger: http://localhost:5000/swagger${NC}"
    echo -e "${CYAN}  - Frontend: http://localhost:4200${NC}"
    echo ""
    write_info "Close each terminal/tab to stop the corresponding service"
    echo ""
}

start_both_services_split_pane() {
    local terminal_type=$(detect_terminal)
    
    case $terminal_type in
        windows-terminal)
            write_info "Detected Windows Terminal (WSL) - launching services in split panes..."
            echo ""
            
            # Create temporary script files to avoid semicolon parsing issues with wt.exe
            temp_backend="/tmp/markit-backend-$$.sh"
            temp_frontend="/tmp/markit-frontend-$$.sh"
            
            # Backend script
            cat > "$temp_backend" << 'BACKEND_SCRIPT'
#!/bin/bash
cd "BACKEND_DIR_PLACEHOLDER"
echo -e '\033[0;36mStarting Backend API...\033[0m'
echo ''
echo -e '\033[1;33mBackend will be available at:\033[0m'
echo -e '\033[0;36m  - API: http://localhost:5000\033[0m'
echo -e '\033[0;36m  - Swagger: http://localhost:5000/swagger\033[0m'
echo ''
dotnet run --launch-profile Development
BACKEND_SCRIPT
            sed -i.bak "s|BACKEND_DIR_PLACEHOLDER|$BACKEND_DIR|g" "$temp_backend" && rm -f "$temp_backend.bak"
            chmod +x "$temp_backend"
            
            # Frontend script
            cat > "$temp_frontend" << 'FRONTEND_SCRIPT'
#!/bin/bash
cd "FRONTEND_DIR_PLACEHOLDER"
echo -e '\033[0;36mStarting Frontend...\033[0m'
echo ''
echo -e '\033[1;33mFrontend will be available at:\033[0m'
echo -e '\033[0;36m  - http://localhost:4200\033[0m'
echo ''
pnpm start
FRONTEND_SCRIPT
            sed -i.bak "s|FRONTEND_DIR_PLACEHOLDER|$FRONTEND_DIR|g" "$temp_frontend" && rm -f "$temp_frontend.bak"
            chmod +x "$temp_frontend"
            
            # Launch Windows Terminal with split panes using script files
            wt.exe new-tab --title "Backend API" bash "$temp_backend" \; split-pane -V --title "Frontend" bash "$temp_frontend" 2>/dev/null || {
                write_warning "Failed to launch Windows Terminal split panes"
                write_info "Falling back to separate windows..."
                rm -f "$temp_backend" "$temp_frontend"
                start_both_services_separate_windows
                return
            }
            
            # Clean up temporary script files after successful launch
            rm -f "$temp_backend" "$temp_frontend"
            
            write_success "Services launched in Windows Terminal split panes"
            echo ""
            echo -e "${YELLOW}Services will be available at:${NC}"
            echo -e "${CYAN}  - Backend API: http://localhost:5000${NC}"
            echo -e "${CYAN}  - Swagger: http://localhost:5000/swagger${NC}"
            echo -e "${CYAN}  - Frontend: http://localhost:4200${NC}"
            echo ""
            write_info "A new Windows Terminal tab with two panes has been created"
            write_info "Press Ctrl+C in each pane to stop the corresponding service"
            echo ""
            ;;
        tmux)
            write_info "Detected tmux - launching services in split panes..."
            echo ""
            
            # Create new tmux session with split panes
            tmux new-session -d -s markit-dev "cd '$BACKEND_DIR' && echo 'Starting Backend API...' && echo '' && dotnet run --launch-profile Development" 2>/dev/null || {
                write_warning "Failed to create tmux session"
                write_info "Falling back to separate windows..."
                start_both_services_separate_windows
                return
            }
            
            tmux split-window -h -t markit-dev "cd '$FRONTEND_DIR' && echo 'Starting Frontend...' && echo '' && pnpm start" 2>/dev/null
            tmux attach-session -t markit-dev 2>/dev/null
            ;;
        *)
            write_info "Using separate windows/tabs..."
            echo ""
            start_both_services_separate_windows
            ;;
    esac
}

start_database() {
    write_info "Starting PostgreSQL Docker container..."
    
    # Check if Docker is available
    if ! command -v docker &> /dev/null; then
        write_warning "Docker not available. Ensure PostgreSQL is running manually."
        return
    fi
    
    # Check if container exists
    if ! docker ps -a --filter "name=markit-postgres" --format "{{.Names}}" 2>/dev/null | grep -q "markit-postgres"; then
        write_error "PostgreSQL container not found."
        write_info "Please run the setup script first: ./setup.sh"
        exit 1
    fi
    
    # Check if container is running
    if docker ps --filter "name=markit-postgres" --format "{{.Status}}" 2>/dev/null | grep -q "Up"; then
        write_success "PostgreSQL container is already running"
    else
        write_info "Starting existing PostgreSQL container..."
        if docker start markit-postgres > /dev/null 2>&1; then
            write_success "PostgreSQL container started"
        else
            write_error "Failed to start PostgreSQL container."
            write_info "Ensure Docker is running and the container is properly configured."
            exit 1
        fi
    fi
}

start_backend() {
    write_info "Starting Backend API..."
    echo ""
    echo -e "${YELLOW}Backend will be available at:${NC}"
    echo -e "${CYAN}  - API: http://localhost:5000${NC}"
    echo -e "${CYAN}  - Swagger: http://localhost:5000/swagger${NC}"
    echo ""
    write_info "Press Ctrl+C to stop the backend"
    echo ""
    
    cd "$BACKEND_DIR"
    dotnet run --launch-profile "Development"
}

start_frontend() {
    write_info "Starting Frontend..."
    echo ""
    echo -e "${YELLOW}Frontend will be available at:${NC}"
    echo -e "${CYAN}  - http://localhost:4200${NC}"
    echo ""
    write_info "Press Ctrl+C to stop the frontend"
    echo ""
    
    cd "$FRONTEND_DIR"
    pnpm start
}

start_all_services() {
    echo ""
    echo -e "${CYAN}╔════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║                                        ║${NC}"
    echo -e "${CYAN}║      Starting Markit Services          ║${NC}"
    echo -e "${CYAN}║                                        ║${NC}"
    echo -e "${CYAN}╚════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${YELLOW}Choose which service to run:${NC}"
    echo -e "${GRAY}  1. Backend API (http://localhost:5000)${NC}"
    echo -e "${GRAY}  2. Frontend (http://localhost:4200)${NC}"
    echo -e "${GRAY}  3. Both (recommended: use separate terminals)${NC}"
    echo ""
    
    read -p "Enter your choice (1-3): " choice
    
    case $choice in
        1)
            start_backend
            ;;
        2)
            start_frontend
            ;;
        3)
            start_both_services_split_pane
            ;;
        *)
            write_error "Invalid choice"
            exit 1
            ;;
    esac
}

# Main Script
if [[ "$SHOW_HELP" == true ]]; then
    show_help
fi

test_configuration

# Validate flag conflicts
if [[ "$RUN_ALL" == true ]] && ([[ "$RUN_BACKEND" == true ]] || [[ "$RUN_FRONTEND" == true ]]); then
    write_error "Cannot use --all with --backend or --frontend flags"
    write_info "Use either:"
    echo -e "  ${YELLOW}./run.sh --all${NC}"
    echo -e "  ${YELLOW}./run.sh --backend${NC}"
    echo -e "  ${YELLOW}./run.sh --frontend${NC}"
    exit 1
fi

# Determine what to run
if [[ "$RUN_ALL" == true ]]; then
    # Run both services in split panes or separate windows
    if [[ "$RUN_DOCKER" == true ]]; then
        start_database
        echo ""
    fi
    start_both_services_split_pane
elif [[ "$RUN_BACKEND" == false && "$RUN_FRONTEND" == false && "$RUN_DOCKER" == false ]]; then
    # No specific service selected, run interactive mode
    start_all_services
else
    # Specific services selected
    if [[ "$RUN_DOCKER" == true ]]; then
        start_database
    fi
    
    if [[ "$RUN_BACKEND" == true && "$RUN_FRONTEND" == true ]]; then
        write_warning "Cannot run both Backend and Frontend in the same terminal"
        write_info "Please use separate terminals or the --all flag:"
        echo -e "  ${YELLOW}Terminal 1: ./run.sh --backend${NC}"
        echo -e "  ${YELLOW}Terminal 2: ./run.sh --frontend${NC}"
        echo -e "  ${GREEN}Or: ./run.sh --all${NC}"
        exit 1
    elif [[ "$RUN_BACKEND" == true ]]; then
        start_backend
    elif [[ "$RUN_FRONTEND" == true ]]; then
        start_frontend
    fi
fi
fi
