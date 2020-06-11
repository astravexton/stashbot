param (
    [Array]$Builds = @("win-x64", "win-arm64", "linux-x64", "linux-musl-x64", "linux-arm64", "osx-x64")
)

$originalPath = Get-Location

$projectName = "StashBot"
$projectLocation = Get-Item "../src/$projectName"
$csprojLocation = Get-Item "$projectLocation/$projectName.csproj"
$meta = (Select-Xml -Path $csprojLocation -XPath "/Project" | Select-Object -ExpandProperty Node).PropertyGroup

$appVersionPrefix = $meta.VersionPrefix
$appVersionSuffix = [String]::IsNullOrEmpty($meta.VersionSuffix) ? "" : ".$($meta.VersionSuffix)"
$appFramework = $meta.TargetFramework
$appVersion = "$appVersionPrefix$appVersionSuffix"

function Build
{
    param (
        [Parameter(Mandatory=$true)][string]$Version,
        [Parameter(Mandatory=$true)][string]$RID
    )

    Write-Output "⚙️  Building $Version for $RID..."

    Set-Location $projectLocation

    $platform = $RID.Replace("linux-musl", "linux.musl").Replace("osx", "macos")
    $extension = $RID.StartsWith("win") ? "exe" : "bin"
    $builtFilename = $RID.StartsWith("win") ? "$projectName.exe" : "$projectName"
    $outputFilename = "StashBot-$Version-$platform.$extension"
    $outputLocation = "../../build/$projectName/$Version"

    dotnet publish -r $RID -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true

    $builtLocation = Get-Item "bin/Release/$appFramework/$RID/publish/$builtFilename"
    New-Item -Type Directory -Force $outputLocation | Out-Null
    Move-Item $builtLocation "$outputLocation/$outputFilename" -Force

    Set-Location $originalPath
}

foreach ($build in $Builds)
{
    Build -Version $appVersion -RID $build
}

Remove-Item -Path "$projectLocation/bin/Release" -Recurse -Force | Out-Null