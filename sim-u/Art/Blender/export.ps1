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

Write-Host "**************************************************************************"
Write-Host "Exporting Blender assets to .FBX with Unity coordinate system corrections."
Write-Host "Using Blender at path '$blenderPath'"
Write-Host "**************************************************************************"
Write-Host ""

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
    $fullName = $blenderFile.FullName
    $relativePath = $fullName.Substring($PSScriptRoot.Length + 1)
    Write-Host "Exporting '$relativePath' -----------------------------------"

    # The blender script also checks if the output file already exists.
    # But it's faster to skip here. So we are checking twice.
    $relativeDirectory = [System.IO.Path]::GetDirectoryName($relativePath)
    $outputNoExt = [System.IO.Path]::GetFileNameWithoutExtension($relativePath)
    $relativeOutput = [System.IO.Path]::Combine($relativeDirectory, $outputNoExt + ".fbx")
    $outputFile = [System.IO.Path]::Combine($outputPath, $relativeOutput)

    if ([System.IO.File]::Exists($outputFile))
    {
        Write-Host " [SKIPPING] Output '$relativeOutput' already exists! Delete file to re-export."
        continue
    }

    Invoke-Expression '& "$blenderPath" -b "$fullName" --python "$exportScript" -- "$outputPath"'

    $exportCount = $exportCount + 1
}

Pop-Location

Write-Host "Complete! Exported $exportCount files."