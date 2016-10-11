[CmdletBinding()]

param(
    [Parameter(Mandatory=$True)]
    [string]$build,
    [Parameter()]
    [string]$targetFolder = $Env:BUILD_STAGINGDIRECTORY
)
    Write-Verbose -Verbose ('build: ' + $build)
    Write-Verbose -Verbose ('targetFolder: ' + $targetFolder)
    Write-Verbose -Verbose ('appendBuildNumberVersion: ' + $appendBuildNumberVersion)

    $tfsUrl = $Env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI + $Env:SYSTEM_TEAMPROJECT
    $definitionUrl = ($tfsUrl + '/_apis/build/definitions?api-version=2.0&name=' + $build)    

    $buildDefinitions = Invoke-RestMethod -Uri $definitionUrl -Method GET -Headers @{
        Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN"    
    }
        
    $buildDefinitionId = ($buildDefinitions.value).id;

    $tfsGetLatestCompletedBuildUrl = $tfsUrl + '/_apis/build/builds?definitions=' + $buildDefinitionId + '&statusFilter=completed&resultFilter=succeeded&$top=1&api-version=2.0'

    $builds = Invoke-RestMethod -Uri $tfsGetLatestCompletedBuildUrl -Method GET -Headers @{
        Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN"    
    }
    $buildId = ($builds.value).id;    

    $dropArchiveDestination = Join-path $targetFolder "drop.zip"


    #build URI for buildNr
    $buildArtifactsURI = $tfsURL + '/_apis/build/builds/' + $buildId + '/artifacts?api-version=2.0'

    #get artifact downloadPath
    $artifactURI = (Invoke-RestMethod -Uri $buildArtifactsURI -Method GET -Headers @{
        Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN"    
    }).Value.Resource.downloadUrl

    #download ZIP
    Invoke-WebRequest -uri $artifactURI -OutFile $dropArchiveDestination -Headers @{
        Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN"    
    }

    #unzip
    Add-Type -assembly 'system.io.compression.filesystem'
    [io.compression.zipfile]::ExtractToDirectory($dropArchiveDestination, $targetFolder)

    Write-Verbose -Verbose ('Build artifacts extracted into ' + $targetFolder)