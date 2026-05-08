# Generate a secure JWT key for Markit API
# Usage: .\generate-jwt-key.ps1

function Generate-SecureKey {
    param (
        [int]$Length = 64
    )
    
    # Calculate required bytes for URL-safe base64 encoding
    # Base64 encodes 3 bytes to 4 characters, so we need (Length * 3 / 4) bytes
    # Add extra to ensure we have enough after conversion
    $byteCount = [Math]::Ceiling($Length * 3 / 4) + 3
    $bytes = New-Object byte[] $byteCount
    
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
        
        # Convert to base64 and make URL-safe
        $base64 = [Convert]::ToBase64String($bytes)
        $urlSafeKey = $base64.Replace('+', '-').Replace('/', '_').TrimEnd('=')
        
        return $urlSafeKey.Substring(0, $Length)
    } finally {
        $rng.Dispose()
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Markit JWT Key Generator" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$jwtKey = Generate-SecureKey -Length 64

Write-Host "Generated JWT Key (64 characters):" -ForegroundColor Green
Write-Host ""
Write-Host $jwtKey -ForegroundColor Yellow
Write-Host ""
Write-Host "Copy this key and use it when setting up your Markit configuration." -ForegroundColor Gray
Write-Host "Keep this key secure and never commit it to version control!" -ForegroundColor Red
Write-Host ""
