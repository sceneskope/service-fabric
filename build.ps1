Write-Output "build: Build started"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
$env:NUGET_XMLDOC_MODE="skip"

$BuildConfiguration="Release"
$ArtifactStagingDirectory="$PSScriptRoot\artifacts"

if(Test-Path $ArtifactStagingDirectory) {
    Write-Output "build: Cleaning "
    Remove-Item $ArtifactStagingDirectory -Force -Recurse
}
Get-ChildItem -rec -Filter bin | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Filter obj | Remove-Item -Recurse -Force

$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and $revision -ne "local"]

if (($env:APPVEYOR_BUILD_VERSION -ne $NULL) -and ($suffix -eq "")) {
    $modifiedVersion = $env:APPVEYOR_BUILD_VERSION
} else {
    $autoVersion = [math]::floor((New-TimeSpan $(Get-Date) $(Get-Date -month 1 -day 1 -year 2016 -hour 0 -minute 0 -second 0)).TotalMinutes * -1).ToString() + "-" + (Get-Date).ToString("ss")
    $modifiedVersion = "1.3.0-beta-$autoVersion"
    
}

dotnet --version
dotnet restore /p:Version=$modifiedVersion
if($LASTEXITCODE -ne 0) { exit 1 }
dotnet build --configuration ${BuildConfiguration} /p:Version=$modifiedVersion
if($LASTEXITCODE -ne 0) { exit 1 }

$projects = $(Get-ChildItem src -rec -filter *.csproj | ForEach-Object { $_.FullName })
foreach ($project in $projects) {
    dotnet pack --no-build --configuration ${BuildConfiguration} --output ${ArtifactStagingDirectory} /p:Version=$modifiedVersion $project
    if($LASTEXITCODE -ne 0) { exit 1 }
}

if (($env:APPVEYOR_BUILD_VERSION -ne $NULL) -and ($suffix -eq "")) {
    Write-Host "Built ok"
} else {
    Write-Host "Copying to local publish directory"
    Copy-Item $ArtifactStagingDirectory\*.nupkg c:\users\nick\source\packages

}