param(    
    [string] $Configuration = "Release",
    [switch] $SkipInit = $false,    
    [switch] $SkipTest = $false
)

Remove-Item .\TestResults\* -Recurse -ErrorAction SilentlyContinue

if (-not $SkipInit)
{
    .\Restore-Packages.ps1
}

$params = @("..\carbon-server\CarbonServer.sln", "/p:Configuration=$Configuration;Platform=""x64""", "/v:m")
& "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe" $params
if ($LASTEXITCODE -ne 0)
{
    throw "Build failed";
}

if (-not $SkipTest)
{
    $params = @("..\carbon-server\Carbon.Test.Unit\bin\$Configuration\Carbon.Test.Unit.dll",`
            "..\carbon-server\Carbon.Test.Integration\bin\$Configuration\Carbon.Test.Integration.dll",`
            "..\carbon-server\Carbon.Test.Performance\bin\$Configuration\Carbon.Test.Performance.dll",`
            "/platform:x64", "/parallel", "/logger:trx")        
                             
& "$env:VS140COMNTOOLS..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" $params
}