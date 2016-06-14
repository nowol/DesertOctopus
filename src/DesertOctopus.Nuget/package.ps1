param (
	[ValidateSet("Debug", "Release")]
    [string]$compileMode
)

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
	$dllFile = Join-Path $scriptFolder "bin\$($compileMode)\$($dllName)"
	$info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllFile)

	if (!$info) {
		throw "Cannot get file version for $dllFile"
	}

	$replacements = @{}
	$replacements["id"] = $info.ProductName
	$replacements["version"] = $info.ProductVersion
	$replacements["title"] = $info.ProductName
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

function ValidateNugetPackageDependencies([String] $nuspec, [String] $projectFolder) {
	
	$nuspecDoc = new-object System.Xml.XmlDocument
	$nuspecDoc.load($nuspec)
	
	$packagesCfg = new-object System.Xml.XmlDocument
	$packagesCfg.load( (Join-Path $projectFolder "packages.config") )

	$packages = $packagesCfg.SelectNodes("//package[not(contains(@id,'Analyzer'))]")
	$groups = $nuspecDoc.SelectNodes("/package/metadata/dependencies/group")

	foreach($group in $groups) {
		if ($group.SelectNodes("dependency").Count -ne $packages.Count) {
			throw "Group $group contains different number of packages than packages.config"
		}

		foreach($package in $packages) {
			$dependency = $group.SelectSingleNode("dependency[@id='$($package.Id)']")
			if (!$dependency) {
				throw "Could not find reference to package $($package.Id) in group $($group.targetFramework)"
			}

			if ($dependency.Version -ne $package.Version) {
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


UpdateNuspec (Join-Path $scriptFolder "DesertOctopus.nuspec") "DesertOctopus.dll"
ValidateNugetPackageDependencies (Join-Path $scriptFolder "DesertOctopus.nuspec") (Join-Path $scriptFolder "..\DesertOctopus")
PackageNuget (Join-Path $scriptFolder "DesertOctopus.nuspec")