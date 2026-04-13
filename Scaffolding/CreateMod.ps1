param(
    [Parameter(Mandatory=$true)]
    [string]$ModName,

    [Parameter(Mandatory=$false)]
    [string]$GameDir
)

$ErrorActionPreference = "Stop"

# --- Game directory resolution ---
# Check for cached path from a previous run
$ConfigFile = Join-Path $PSScriptRoot ".game-directory"

if (-not $GameDir -and (Test-Path $ConfigFile)) {
    $GameDir = (Get-Content $ConfigFile -Raw).Trim()
    Write-Host "Using cached game directory: $GameDir" -ForegroundColor DarkGray
}

if (-not $GameDir) {
    Write-Host "ERROR: -GameDir is required on first run." -ForegroundColor Red
    Write-Host "Example: .\CreateMod.ps1 -ModName MyMod -GameDir ""E:\SteamLibrary\steamapps\common\Software Inc""" -ForegroundColor Yellow
    exit 1
}

# Validate that the path looks like a Software Inc installation
$AssemblyPath = Join-Path $GameDir "Software Inc_Data\Managed\Assembly-CSharp.dll"
if (-not (Test-Path $AssemblyPath)) {
    Write-Host "ERROR: '$GameDir' does not look like a Software Inc installation." -ForegroundColor Red
    Write-Host "Expected to find: $AssemblyPath" -ForegroundColor Yellow
    exit 1
}

# Cache the valid path for future runs
Set-Content $ConfigFile $GameDir -Encoding UTF8

# --- Mod name validation ---
if ($ModName -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    Write-Host "ERROR: ModName must be a valid C# identifier (letters, numbers, underscores, cannot start with a number)." -ForegroundColor Red
    Write-Host "Got: '$ModName'" -ForegroundColor Yellow
    exit 1
}

$TemplatesDir = Join-Path $PSScriptRoot "Templates"
$TargetDir = Join-Path (Split-Path (Split-Path $PSScriptRoot)) $ModName

if (Test-Path $TargetDir) {
    Write-Host "Directory $TargetDir already exists. Please choose a different mod name." -ForegroundColor Red
    exit 1
}

Write-Host "Creating ModFramework Mod: $ModName" -ForegroundColor Cyan
Write-Host "Game Directory: $GameDir" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null

$Guid = [guid]::NewGuid().ToString().ToUpper()

Write-Host "Copying templates..."
# 1. Main Behaviour
$BehaviourContent = Get-Content (Join-Path $TemplatesDir "MainBehaviour.cs_template") -Raw
$BehaviourContent = $BehaviourContent.Replace('{MOD_NAME}', $ModName)
Set-Content (Join-Path $TargetDir "${ModName}Behaviour.cs") $BehaviourContent -Encoding UTF8

# 2. ModMeta.json
$MetaContent = Get-Content (Join-Path $TemplatesDir "ModMeta.json_template") -Raw
$MetaContent = $MetaContent.Replace('{MOD_NAME}', $ModName)
Set-Content (Join-Path $TargetDir "ModMeta.json") $MetaContent -Encoding UTF8

# 3. .csproj (with game directory substitution)
$CsprojContent = Get-Content (Join-Path $TemplatesDir "Mod.csproj_template") -Raw
$CsprojContent = $CsprojContent.Replace('{MOD_NAME}', $ModName)
$CsprojContent = $CsprojContent.Replace('{NEW_GUID}', $Guid)
$CsprojContent = $CsprojContent.Replace('{GAME_DIRECTORY}', $GameDir)
Set-Content (Join-Path $TargetDir "${ModName}.csproj") $CsprojContent -Encoding UTF8

# 4. meta.tyd (required by Software Inc for mod discovery)
$TydContent = Get-Content (Join-Path $TemplatesDir "meta.tyd_template") -Raw
$TydContent = $TydContent.Replace('{MOD_NAME}', $ModName)
Set-Content (Join-Path $TargetDir "meta.tyd") $TydContent -Encoding UTF8

Write-Host ""
Write-Host "Successfully generated '$ModName' at $TargetDir !" -ForegroundColor Green
Write-Host "Game references point to: $GameDir" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:"
Write-Host "1. Add existing project $ModName.csproj to your Visual Studio Solution."
Write-Host "2. Make sure ModFramework is built first."
Write-Host "3. Build your new mod. The post-build event will automatically copy it to your local game's Mods folder."
Write-Host ""
