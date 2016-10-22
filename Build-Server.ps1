param(    
    [string] $BuildNumber, 
    [string] $Configuration = "Release",
    [switch] $SkipBuild = $false,    
    [switch] $SkipInit = $false,    
    [switch] $SkipTest = $false
)

try
{
    Push-Location $PSScriptRoot

    Remove-Item .\TestResults\* -Recurse -ErrorAction SilentlyContinue
    Remove-Item .\target\* -Recurse -ErrorAction SilentlyContinue

    if (-not $SkipInit)
    {
        .\Restore-Packages.ps1
    }

    $msbuild = "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"

    if (-not $SkipBuild)
    {
        $params = @(".\CarbonServer.sln", "/p:Configuration=$Configuration;Platform=x64", "/v:m")
        & $msbuild $params
        if ($LASTEXITCODE -ne 0)
        {
            throw "Build failed";
        }
    }

    if (-not $SkipTest)
    {
        $params = @(".\Carbon.Test.Unit\bin\$Configuration\Carbon.Test.Unit.dll",`
                ".\Carbon.Test.Integration\bin\$Configuration\Carbon.Test.Integration.dll",`
                ".\Carbon.Test.Performance\bin\$Configuration\Carbon.Test.Performance.dll",`
                "/platform:x64", "/parallel", "/logger:trx")        
                             
        & "$env:VS140COMNTOOLS..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" $params
    }

    Write-Host "Packaging project..."        
    $params = @(".\Carbon.Fabric\Carbon.Fabric.sfproj", "/target:Package", "/p:Platform=x64;Configuration=$Configuration;PackageLocation=..\target", "/v:m")
    & $msbuild $params
    if ($LASTEXITCODE -ne 0)
    {
        throw "Packaging failed";
    }

    Copy-Item .\Carbon.Fabric\ApplicationParameters .\target -Recurse -Force
    Copy-Item .\Carbon.Fabric\PublishProfiles .\target -Recurse -Force
    Copy-Item .\Carbon.Deployment\Templates .\target -Recurse -Force

    if ($BuildNumber)
    {
        $BuildNumber > .\target\version
    }
}
finally
{
    Pop-Location
}