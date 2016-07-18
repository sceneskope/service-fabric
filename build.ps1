echo "build: Build started"

Push-Location $PSScriptRoot

if(Test-Path .\artifacts) {
    echo "build: Cleaning .\artifacts"
    Remove-Item .\artifacts -Force -Recurse
}
Get-ChildItem -rec -Filter bin | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Filter obj | Remove-Item -Recurse -Force

$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and $revision -ne "local"]

$modifyVersion = $false
$modifiedVersion = "1.0.0-*"
if (($env:APPVEYOR_BUILD_VERSION -ne $NULL) -and ($suffix -eq "")) {
    $modifyVersion = $true
    $modifiedVersion = $env:APPVEYOR_BUILD_VERSION

    Get-ChildItem -Path . -Filter project.json -Recurse |
    ForEach-Object {
        $content = get-content $_.FullName
        $content = $content.Replace("1.0.0-*", "$modifiedVersion")
        Set-Content $_.FullName $content -Encoding UTF8
    }
}



& dotnet restore --no-cache

foreach ($test in ls test/*.Tests) {
    Push-Location $test

    echo "build: Testing project in $test"

    & dotnet test
    if($LASTEXITCODE -ne 0) { Pop-Location; exit 3 }

    Pop-Location
}


echo "build: Version $modifiedVersion $suffix"
dotnet build src/**/project.json --version-suffix=$suffix -c Release
if($LASTEXITCODE -ne 0) { exit 1 }

foreach ($src in ls src/*) {
    Push-Location $src

    echo "build: Packaging project in $src"

    & dotnet pack --no-build -c Release -o ..\..\artifacts --version-suffix=$suffix
    if($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }

    Pop-Location
}

if ($modifyVersion) {
    Get-ChildItem -Path . -Filter project.json -Recurse |
    ForEach-Object {
        $content = get-content $_.FullName
        $content = $content.Replace("$modifiedVersion", "1.0.0-*")
        Set-Content $_.FullName $content -Encoding UTF8
    }
}

Pop-Location

