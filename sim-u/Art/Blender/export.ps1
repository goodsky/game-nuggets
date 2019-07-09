<#
    Export the Blender files into FBX files with the correct coordinate system for Unity.
    author: goodsky
#>

param(
    <# (optional) Filter the export to include only files that match this filter string. #>
    [Parameter(Position=0)]$fileFilter
)

$blenderPath = "${env:ProgramFiles}\Blender Foundation\Blender\blender.exe"
$outputPath = "$PSScriptRoot\..\..\Assets\Resources\Models\"
$exportScript = "$PSScriptRoot\Tools\export-fbx.py"

if (!(Test-Path "$blenderPath"))
{
    Write-Host "Could not find Blender installed on this machine!"
    exit
}

Write-Host "Exporting Blender assets to .FBX with Unity coordinate system corrections."
Write-Host "Using Blender at path '$blenderPath'"

Push-Location $PSScriptRoot

if ([String]::IsNullOrEmpty($fileFilter))
{
    $fileFilter = "*"
}

if (!$fileFilter.EndsWith(".blend"))
{
    $fileFilter = $fileFilter + ".blend"
}

$exportCount = 0
$blenderFiles = Get-ChildItem -Path . -Filter $fileFilter -Recurse
foreach ($blenderFile in $blenderFiles)
{
    Write-Host "Exporting '$blenderFile'..."

    $fullName = $blenderFile.FullName
    Invoke-Expression '& "$blenderPath" -b "$fullName" --python "$exportScript" -- "$outputPath"'

    $exportCount = $exportCount + 1
}

Pop-Location

Write-Host "Complete! Exported $exportCount files."