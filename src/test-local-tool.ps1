$ErrorActionPreference = "Stop"

$projectPath = "DiffLog.csproj"
$packOutput = "bin\Release"

try {
  dotnet tool uninstall --global DiffLog | Out-Null
  Write-Host "Removed existing global tool DiffLog."
} catch {
  Write-Host "No existing global tool to uninstall."
}

dotnet pack $projectPath -c Release
dotnet tool install --global --add-source $packOutput DiffLog

Write-Host "Installed DiffLog from $packOutput."
