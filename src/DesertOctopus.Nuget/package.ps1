

$ErrorActionPreference = "Stop"


$scriptFolder = $PSScriptRoot
$nugetLocation = Join-Path $scriptFolder "..\..\Tools\nuget\nuget.exe"

<#
	Change nuspec files
		id
		version
		title
		authors
		owners
		description

	Check that packages from packages.config are set in the dependencies

	nuget pack each nuspec file ?

	get VSTS to publish the packages
#>

function UpdateNuspec([String] $nuspec, [String] $dllName)
{
	$dllFile = Join-Path $scriptFolder "bin\Release\$($dllName)"
	$info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllFile)

	if (!$info) {
		throw "Cannot get file version for $dllFile"
	}
	
	$replacements = @{}
	$replacements["id"] = $info.FileDescription
	$replacements["version"] = $info.ProductVersion + ""
	$replacements["title"] = $info.FileDescription
	$replacements["authors"] = $info.CompanyName
	$replacements["owners"] = $info.CompanyName
	$replacements["description"] = $info.Comments
	$replacements["projectUrl"] = "https://github.com/nowol/DesertOctopus"
	$replacements["licenseUrl"] = "https://github.com/nowol/DesertOctopus/blob/master/LICENSE"

	$xdoc = new-object System.Xml.XmlDocument
	$xdoc.load($nuspec)

	$content = Get-Content $nuspec
	foreach ($key in $replacements.Keys) {
		$node = $xdoc.SelectSingleNode("/package/metadata/$($key)")
		$node.InnerText = $replacements[$key]
	}

	$xdoc.Save($nuspec)
}

function UpdateDesertOctopusDependencies([String] $nuspec, [String] $dllName)
{
	$dllFile = Join-Path $scriptFolder "bin\Release\$($dllName)"
	$info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllFile)

	if (!$info) {
		throw "Cannot get file version for $dllFile"
	}

	$xdoc = new-object System.Xml.XmlDocument
	$xdoc.load($nuspec)

	$groups = $xdoc.SelectNodes("/package/metadata/dependencies/group/dependency")
	foreach($group in $groups) {
		if ($group.id.StartsWith("DesertOctopus")) {
			$group.version = $info.ProductVersion
		}
	}
	
	$xdoc.Save($nuspec)
}

function ValidateNugetPackageDependencies([String] $nuspec, [String] $projectFolder) {
	
	$nuspecDoc = new-object System.Xml.XmlDocument
	$nuspecDoc.load($nuspec)
	
	$packagesCfg = new-object System.Xml.XmlDocument
	$packagesCfg.load( (Join-Path $projectFolder "packages.config") )

	$packages = $packagesCfg.SelectNodes("//package[not(contains(@id,'Analyzer')) and not(contains(@id,'DesertOctopus'))]")
	$groups = $nuspecDoc.SelectNodes("/package/metadata/dependencies/group")
	
	foreach($group in $groups) {
		$dependencies = $group.SelectNodes("dependency[not(contains(@id,'Analyzer')) and not(contains(@id,'DesertOctopus'))]")

		if ($dependencies.Count -ne $packages.Count) {
			$packages | fl
			$dependencies | fl

			throw "Group $group contains different number of packages than packages.config"
		}

		foreach($package in $packages) {
			$dependency = $group.SelectSingleNode("dependency[@id='$($package.Id)']")
			if (!$dependency) {
				throw "Could not find reference to package $($package.Id) in group $($group.targetFramework)"
			}

			if (!$dependency.Version.Contains($package.Version)) {
				throw "Different versions for package package $($package.Id).  Expected version $($package.Version) but got  $($dependency.Version) instead."
			}
		}
	}
}

function PackageNuget([String] $nuspec) {
	& $nugetLocation pack $nuspec
}


# clean up

Get-ChildItem $scriptFolder | Where{$_.Name -Match "^.*\.nupkg"} | Remove-Item
Get-ChildItem (Join-Path $scriptFolder "bin\Release\") | Where{$_.Name -Match "^.*\.nupkg"} | Remove-Item

Write-Host "DesertOctopus"
UpdateDesertOctopusDependencies (Join-Path $scriptFolder "DesertOctopus.nuspec") "DesertOctopus.dll"
UpdateNuspec (Join-Path $scriptFolder "DesertOctopus.nuspec") "DesertOctopus.dll"
ValidateNugetPackageDependencies (Join-Path $scriptFolder "DesertOctopus.nuspec") (Join-Path $scriptFolder "..\DesertOctopus")
PackageNuget (Join-Path $scriptFolder "DesertOctopus.nuspec")

Write-Host "DesertOctopus.MammothCache.Common"
UpdateDesertOctopusDependencies (Join-Path $scriptFolder "DesertOctopus.MammothCache.Common.nuspec") "DesertOctopus.MammothCache.Common.dll"
UpdateNuspec (Join-Path $scriptFolder "DesertOctopus.MammothCache.Common.nuspec") "DesertOctopus.MammothCache.Common.dll"
ValidateNugetPackageDependencies (Join-Path $scriptFolder "DesertOctopus.MammothCache.Common.nuspec") (Join-Path $scriptFolder "..\DesertOctopus.MammothCache.Common")
PackageNuget (Join-Path $scriptFolder "DesertOctopus.MammothCache.Common.nuspec")

Write-Host "DesertOctopus.MammothCache"
UpdateDesertOctopusDependencies (Join-Path $scriptFolder "DesertOctopus.MammothCache.nuspec") "DesertOctopus.MammothCache.dll"
UpdateNuspec (Join-Path $scriptFolder "DesertOctopus.MammothCache.nuspec") "DesertOctopus.MammothCache.dll"
ValidateNugetPackageDependencies (Join-Path $scriptFolder "DesertOctopus.MammothCache.nuspec") (Join-Path $scriptFolder "..\DesertOctopus.MammothCache")
PackageNuget (Join-Path $scriptFolder "DesertOctopus.MammothCache.nuspec")


Write-Host "DesertOctopus.MammothCache.Redis"
UpdateDesertOctopusDependencies (Join-Path $scriptFolder "DesertOctopus.MammothCache.Redis.nuspec") "DesertOctopus.MammothCache.Redis.dll"
UpdateNuspec (Join-Path $scriptFolder "DesertOctopus.MammothCache.Redis.nuspec") "DesertOctopus.MammothCache.Redis.dll"
ValidateNugetPackageDependencies (Join-Path $scriptFolder "DesertOctopus.MammothCache.Redis.nuspec") (Join-Path $scriptFolder "..\DesertOctopus.MammothCache.Redis")
PackageNuget (Join-Path $scriptFolder "DesertOctopus.MammothCache.Redis.nuspec")

