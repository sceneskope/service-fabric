[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$BuildNumber = "dev",
    [bool] $CreatePackages = $true,
    [bool] $RunTests = $true,
    [bool] $CopyLocal = $false
)

if (!(Test-Path "./semver.txt")) {
    throw "No semver exists"
}

Write-Host "BuildNumber: $BuildNumber"
Write-Host "CreatePackages: $CreatePackages"
Write-Host "RunTests: $RunTests"

Get-ChildItem -Recurse -Filter bin | Remove-Item -Recurse
Get-ChildItem -Recurse -Filter obj | Remove-Item -Recurse

$packageOutputFolder = "$PSScriptRoot\.nupkgs"

$semVer = Get-Content (Join-Path $PSScriptRoot "semver.txt")
if ($semVer.Contains("-") -and  -not $semVer.EndsWith("-")) {
    throw "Semver with a dash should end in dash"
}
elseif (-not $semVer.EndsWith(".")) {
    throw "Semver should end with a dot"
}
if ($BuildNumber -eq "dev") {
    $autoVersion = [math]::floor((New-TimeSpan $(Get-Date) $(Get-Date -month 1 -day 1 -year 2016 -hour 0 -minute 0 -second 0)).TotalMinutes * -1).ToString() + "-" + (Get-Date).ToString("ss")
    $version = "$semVer-dev-$autoVersion"
    $configuration = "Debug"
}
else {
    $version = "$semVer$BuildNumber"
    $configuration = "Release"
}

Write-Host "Restore"
dotnet restore "/p:Version=$version"
if ($LastExitCode -ne 0) { 
    Write-Host "Error with restore, aborting build." -Foreground "Red"
    Exit 1
}

if ($RunTests) {
    Write-Host "Running tests"
    Get-ChildItem  -Recurse "test\*.csproj" |
        ForEach-Object {
        & dotnet test $_
        if ($LastExitCode -ne 0) { 
            Write-Host "Error with test, aborting build." -Foreground "Red"
            Exit 1
        }
    }
}

Write-Host "Building"
dotnet build -c $configuration "/p:Version=$version"
if ($LastExitCode -ne 0) { 
    Write-Host "Error with build, aborting build." -Foreground "Red"
    Exit 1
}



if ($CreatePackages) {
    Write-Host "Packing"
    mkdir -Force $packageOutputFolder | Out-Null

    Get-ChildItem $packageOutputFolder | Remove-Item
    dotnet pack --no-build --output $packageOutputFolder -c $configuration "/p:Version=$version" 
    if ($LastExitCode -ne 0) { 
        Write-Host "Error with pack, aborting build." -Foreground "Red"
        Exit 1
    }
    if ($CopyLocal) {
        Copy-Item -Path "$packageOutputFolder\*.nupkg" -Destination "$env:HOME\Source\Packages"
    }
}
Write-Host "Complete"

