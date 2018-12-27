
Param(
    [string]$RedisConnectionString,
    [string]$BuildConfiguration = "Debug"


)


$WorkSpaceRoot = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $WorkSpaceRoot "artifacts"
$artifactsTestResults = Join-path $artifacts "TestResults"



function Update-RedisConnectionString() {
	if ($RedisConnectionString) {
        Write-Host "Setting Redis connection string to $RedisConnectionString"

        Get-ChildItem -Recurse -Filter appsettings.json | % {
          $file = $_.FullName

          $jsonContent = Get-Content -Raw $file | ConvertFrom-Json

           if ($jsonContent -and $jsonContent.RedisConnectionString) {
            Write-Host "    Found appsettings.json file $file"
            $jsonContent.RedisConnectionString = $RedisConnectionString
            $jsonContent | ConvertTo-Json | Out-File -FilePath $file
          }
        }
	}
    else {
        Write-Host "Updating Redis connection string is not needed"
    }
}

function Get-GitVersionExpectedVersion() {
    $csprojPath = (Join-Path $WorkSpaceRoot "src\BuildDependencies\BuildDependencies.csproj")
    $content = (Get-Content -Encoding utf8 $csprojPath)
    $csprojContent = [xml]$content

    $p = $csprojContent.SelectSingleNode("//PackageReference[@Include='GitVersion.CommandLine']")
    if ($p -and $p.Attributes["Version"]) {
        return $p.Attributes["Version"].Value
    }

    return $null
}

function Get-GitVersionCommandLinePath() {
    $expectedVersion = Get-GitVersionExpectedVersion
    Write-Host "Expected GitVersion is $expectedVersion"
    $filter = "gitversion.commandline\$expectedVersion\tools\GitVersion.exe"

    $nugetPaths = (dotnet nuget locals -l all).Split([Environment]::NewLine) 
    
    
    foreach ($nugetPath in $nugetPaths) {
        $path = ($nugetPath.Split(":")[2..100] -join ":").Trim()
        $children = Get-ChildItem -Path $path -Filter $filter -ErrorAction SilentlyContinue
                
        if ($children -and $children.Length -gt 0) {
            Write-Host "Found GitVersion: $($children[0].FullName)"
            return $children[0].FullName
        }

        #gitversion.commandline
    }

    
    throw "Unable to find GitVersion command line"
}

function Get-VersionInfos() {
    $gvPath = Get-GitVersionCommandLinePath
    
    $json = (& $gvPath) | ConvertFrom-Json
    
    return @{
        AssemblyVersion = $json.SemVer
        AssemblyInfoVersion = $json.InformationalVersion
        AssemblyFileVersion = $json.AssemblySemFileVer
    }
}


try {
    Set-Location -Path  $WorkSpaceRoot

    #####
    Write-Host "Cleaning repository"
    dotnet clean
    if (Test-Path $artifacts) {
        Write-Host "Removing artifacts folder"
        Remove-Item -Path $artifacts -Force -Recurse
    }



    #####
    Write-Host "Restoring"
    dotnet restore

    
    #####
    Write-Host "GitVersion"

    $gitVersion = Get-VersionInfos
    $gitVersion

    
    #####
    Write-Host "Building"
    dotnet build -c $BuildConfiguration /p:Version=$($gitVersion.AssemblyVersion) /p:FileVersion=$($gitVersion.AssemblySemFileVer) /p:InformationalVersion=$($gitVersion.AssemblyInfoVersion)



    #####
    Update-RedisConnectionString

    #####
    Write-Host "Running unit tests"
    dotnet test -c $BuildConfiguration --no-build --filter Category=Unit .\DesertOctopus.sln /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutputFormat=opencover /p:CoverletOutput=$artifactsTestResults

    #####
    Write-Host "Packing Nuget files"
    dotnet pack -c $BuildConfiguration --no-build --include-symbols /p:Version=$($gitVersion.AssemblyVersion)

}
finally {
    Set-Location -Path  $PSScriptRoot
}



