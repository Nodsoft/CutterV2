# This script makes use of WinGet, so make sure you're running Windows 10 or later.

try
{
    $winget = winget --version
}
catch
{
    Write-Host "WinGet is not installed. Please install Windows Package Manager (WinGet) first."
    return
}

# Detect any existing PostgreSQL installations
if (Get-Process | Where-Object { $_.ProcessName -eq "postgres" }) {
    Write-Host "PostgreSQL is already installed and running. Skipping installation."
    return
}

# Install PostgreSQL

Write-Host "Installing PostgreSQL..."
winget install "postgresql.postgresql"