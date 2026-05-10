#!/bin/bash
# Generate a secure JWT key for Markit API
# Usage: ./generate-jwt-key.sh

generate_secure_key() {
    local length=64
    
    # Generate random bytes and convert to hex (2 hex chars per byte)
    key=$(openssl rand -hex $((length / 2)))
    
    echo "$key"
}

echo ""
echo "====================================="
echo "  Markit JWT Key Generator"
echo "====================================="
echo ""

jwt_key=$(generate_secure_key)

echo "Generated JWT Key (64 characters):"
echo ""
echo "$jwt_key"
echo ""
echo "Copy this key and use it when setting up your Markit configuration."
echo "Keep this key secure and never commit it to version control!"
echo ""
