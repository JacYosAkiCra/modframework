param(
    [Parameter(Mandatory=$true)]
    [string]$ModName
)

$ErrorActionPreference = "Stop"

$TemplatesDir = Join-Path $PSScriptRoot "Templates"
$TargetDir = Join-Path (Split-Path (Split-Path $PSScriptRoot)) $ModName

if (Test-Path $TargetDir) {
    Write-Host "Directory $TargetDir already exists. Please choose a different mod name." -ForegroundColor Red
    exit 1
}

Write-Host "Creating ModFramework Mod: $ModName" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null

$Guid = [guid]::NewGuid().ToString().ToUpper()

Write-Host "Copying templates..."
# 1. Main Behaviour
$BehaviourContent = Get-Content (Join-Path $TemplatesDir "MainBehaviour.cs_template") -Raw
$BehaviourContent = $BehaviourContent -replace '{MOD_NAME}', $ModName
Set-Content (Join-Path $TargetDir "${ModName}Behaviour.cs") $BehaviourContent

# 2. ModMeta.json
$MetaContent = Get-Content (Join-Path $TemplatesDir "ModMeta.json_template") -Raw
$MetaContent = $MetaContent -replace '{MOD_NAME}', $ModName
Set-Content (Join-Path $TargetDir "ModMeta.json") $MetaContent

# 3. .csproj
$CsprojContent = Get-Content (Join-Path $TemplatesDir "Mod.csproj_template") -Raw
$CsprojContent = $CsprojContent -replace '{MOD_NAME}', $ModName
$CsprojContent = $CsprojContent -replace '{NEW_GUID}', $Guid
Set-Content (Join-Path $TargetDir "${ModName}.csproj") $CsprojContent

# 4. meta.tyd (required by Software Inc for mod discovery)
$TydContent = Get-Content (Join-Path $TemplatesDir "meta.tyd_template") -Raw
$TydContent = $TydContent -replace '{MOD_NAME}', $ModName
Set-Content (Join-Path $TargetDir "meta.tyd") $TydContent

Write-Host ""
Write-Host "Successfully generated '$ModName' at $TargetDir !" -ForegroundColor Green
Write-Host "Next Steps:"
Write-Host "1. Add existing project $ModName.csproj to your Visual Studio Solution."
Write-Host "2. Make sure ModFramework is built first."
Write-Host "3. Build your new mod. The post-build event will automatically copy it to your local game's Mods folder."
Write-Host ""
