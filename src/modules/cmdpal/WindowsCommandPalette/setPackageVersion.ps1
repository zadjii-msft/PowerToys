$verPropReadFileLocation = $PSScriptRoot + '/../../../Version.props';

[XML]$verProps = Get-Content $verPropReadFileLocation
$versionNumber = $verProps.Project.PropertyGroup.Version;

Write-Host "Version: " $versionNumber

# Set package version in Package.appxmanifest
$AppManifestWriteFileLocation = $PSScriptRoot + '/Package.appxmanifest';
$AppManifestReadFileLocation = $AppManifestWriteFileLocation;

[XML]$AppManifest = Get-Content $AppManifestReadFileLocation
$AppManifest.Package.Identity.Version = $versionNumber + '.0'
Write-Host "Package version: " $AppManifest.Package.Identity.Version
$AppManifest.Save($AppManifestWriteFileLocation);
