[CmdletBinding(PositionalBinding = $false)]
param(
    [bool] $CreatePackages = $true,
    [bool] $RunTests = $true,
    [bool] $CopyLocal = $false,
    [string] $Configuration = "Release",
    [string] $BuildNumber
)

Push-Location $PSScriptRoot

$envBranch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $env:BUILD_SOURCEBRANCHNAME}[$env:APPVEYOR_REPO_BRANCH -ne $NULL]
$envBuildNumber = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = $BuildNumber}[$env:APPVEYOR_BUILD_NUMBER -ne $NULL]
$packageOutputFolder = "$PSScriptRoot\.nupkgs"

$branch = @{ $true = $envBranch; $false = $(git symbolic-ref --short -q HEAD) }[$envBranch -ne $NULL];
$autoVersion = [math]::floor((New-TimeSpan $(Get-Date) $(Get-Date -month 1 -day 1 -year 2016 -hour 0 -minute 0 -second 0)).TotalMinutes * -1).ToString() + "-" + (Get-Date).ToString("ss")
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $envBuildNumber, 10); $false = "0-local-$autoVersion" }[$envBuildNumber -ne $NULL -and $envBuildNumber -ne ""];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and -not $revision.StartsWith("local")]
$packSuffix = @{ $true=""; $false="--version-suffix=$suffix"}[$suffix -eq ""]
$commitHash = $(git rev-parse --short HEAD)
$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]

Write-Host "build: Branch is $branch"
Write-host "build: Revision is $revision"
Write-Host "build: Suffix is $suffix"
Write-Host "build: Package version suffix is $packSuffix"
Write-Host "build: Build version suffix is $buildSuffix" 
Write-Host "build: Configuration = $Configuration"

Write-Host "Cleaning anything old"
dotnet clean -c $Configuration

Write-Host "Building"
dotnet build -c $Configuration --version-suffix=$buildSuffix
if ($LastExitCode -ne 0) { 
    Write-Host "Error with build, aborting build." -Foreground "Red"
    Exit 1
}

if ($RunTests) {
    Write-Host "Running tests"
    Get-ChildItem  -Recurse "test\*.csproj" |
    ForEach-Object {
        & dotnet test -c $Configuration $_
        if ($LastExitCode -ne 0) { 
            Write-Host "Error with test, aborting build." -Foreground "Red"
            Exit 1
        }
    }
}

if ($CreatePackages) {
    Write-Host "Packing"
    mkdir -Force $packageOutputFolder | Out-Null

    Get-ChildItem $packageOutputFolder | Remove-Item
    dotnet pack --output $packageOutputFolder -c $Configuration --include-symbols --no-build $packSuffix
    if ($LastExitCode -ne 0) { 
        Write-Host "Error with pack, aborting build." -Foreground "Red"
        Exit 1
    }
    if ($CopyLocal) {
        Copy-Item -Path "$packageOutputFolder\*.nupkg" -Destination "$env:HOME\Source\Packages"
    }
}
Write-Host "Complete"


Pop-Location
