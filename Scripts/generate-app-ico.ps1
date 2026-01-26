<#
.SYNOPSIS
    Generates app.ico for Windows from the master app icon PNG.

.DESCRIPTION
    Creates a multi-resolution ICO file containing 16, 24, 32, 48, 64, 128, and 256 pixel icons.
    Requires ImageMagick (magick.exe) to be installed and available in PATH.

.EXAMPLE
    .\generate-app-ico.ps1

.NOTES
    Install ImageMagick: winget install ImageMagick.ImageMagick
    Or download from: https://imagemagick.org/script/download.php
#>

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

$sourcePng = Join-Path $repoRoot "Assets\AppIcon\Source\appicon-master.png"
$outputIco = Join-Path $repoRoot "Assets\AppIcon\Windows\app.ico"

# Check if ImageMagick is available
$magick = Get-Command "magick" -ErrorAction SilentlyContinue
if (-not $magick) {
    Write-Host ""
    Write-Host "ERROR: ImageMagick (magick.exe) is not found in PATH." -ForegroundColor Red
    Write-Host ""
    Write-Host "To install ImageMagick on Windows:" -ForegroundColor Yellow
    Write-Host "  Option 1: winget install ImageMagick.ImageMagick"
    Write-Host "  Option 2: choco install imagemagick"
    Write-Host "  Option 3: Download from https://imagemagick.org/script/download.php"
    Write-Host ""
    Write-Host "After installation, restart your terminal and run this script again."
    exit 1
}

# Check if source PNG exists
if (-not (Test-Path $sourcePng)) {
    Write-Host "ERROR: Source image not found: $sourcePng" -ForegroundColor Red
    exit 1
}

Write-Host "Generating app.ico from appicon-master.png..." -ForegroundColor Cyan
Write-Host "Source: $sourcePng"
Write-Host "Output: $outputIco"

# ICO sizes for Windows (16, 24, 32, 48, 64, 128, 256)
# Using PNG compression inside ICO for sizes >= 48
$sizes = @(16, 24, 32, 48, 64, 128, 256)

# Create temporary directory for intermediate files
$tempDir = Join-Path $env:TEMP "app-ico-gen-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    $tempFiles = @()

    foreach ($size in $sizes) {
        $tempFile = Join-Path $tempDir "icon-$size.png"
        $tempFiles += $tempFile

        Write-Host "  Resizing to ${size}x${size}..." -ForegroundColor Gray

        # Resize with high quality
        & magick $sourcePng -resize "${size}x${size}" -gravity center -background transparent -extent "${size}x${size}" $tempFile

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to resize image to ${size}x${size}"
        }
    }

    Write-Host "  Combining into ICO..." -ForegroundColor Gray

    # Combine all sizes into single ICO
    & magick @tempFiles $outputIco

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create ICO file"
    }

    Write-Host ""
    Write-Host "SUCCESS: app.ico created at $outputIco" -ForegroundColor Green
    Write-Host "Sizes included: $($sizes -join ', ')" -ForegroundColor Gray

} finally {
    # Cleanup temp files
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force
    }
}
