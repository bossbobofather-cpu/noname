param(
    [switch]$BuildSolution,
    [string]$SolutionPath = "noname.sln",
    [string]$XmlSource = "Docs/Assembly-CSharp.xml",
    [string]$BuildOutput = "Temp/bin/Debug",
    [string]$XmlFileName = "Assembly-CSharp.xml"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($BuildSolution)
{
    Write-Host "Building solution $SolutionPath..."
    dotnet build $SolutionPath
}

if (-not (Test-Path $XmlSource))
{
    throw "XML documentation file '$XmlSource'이(가) 존재하지 않습니다. 먼저 Unity에서 빌드하거나 dotnet build를 실행하세요."
}

$destinationDir = Join-Path -Path $PSScriptRoot -ChildPath $BuildOutput
if (-not (Test-Path $destinationDir))
{
    New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
}

$destinationPath = Join-Path -Path $destinationDir -ChildPath $XmlFileName
Copy-Item -Path $XmlSource -Destination $destinationPath -Force
Write-Host "Copied XML doc to $destinationPath"
