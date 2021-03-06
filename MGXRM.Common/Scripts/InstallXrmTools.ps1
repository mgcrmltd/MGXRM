﻿param (
    [parameter(position = 0)]
    [string]$devToolsDirectory = "c:\devtools"
)

function Get-ScriptDirectory 
{ 
    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 
}

$scriptDir =  Get-ScriptDirectory
set-location $scriptDir 

$nugetDir = Join-Path $devToolsDirectory "nuget"

new-item -ItemType directory $devToolsDirectory -ErrorAction Ignore | Out-Null
new-item -ItemType directory $nugetDir -ErrorAction Ignore | Out-Null
  
$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$targetNugetExe = join-path $nugetDir "nuget.exe"

Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe -ErrorAction SilentlyContinue
Set-Alias nuget $targetNugetExe -Scope Global -Verbose

set-location $devToolsDirectory
Remove-Item .\Tools -Force -Recurse -ErrorAction Ignore  | Out-Null

##
##Download Plugin Registration Tool
##
nuget install Microsoft.CrmSdk.XrmTooling.PluginRegistrationTool -O .\Tools
md .\Tools\PluginRegistration  | Out-Null
$prtFolder = Get-ChildItem ./Tools | Where-Object {$_.Name -match 'Microsoft.CrmSdk.XrmTooling.PluginRegistrationTool.'}
move .\Tools\$prtFolder\tools\*.* .\Tools\PluginRegistration
Remove-Item .\Tools\$prtFolder -Force -Recurse | Out-Null

##
##Download CoreTools
##
nuget install  Microsoft.CrmSdk.CoreTools -O .\Tools
md .\Tools\CoreTools  | Out-Null
$coreToolsFolder = Get-ChildItem ./Tools | Where-Object {$_.Name -match 'Microsoft.CrmSdk.CoreTools.'}
move .\Tools\$coreToolsFolder\content\bin\coretools\*.* .\Tools\CoreTools
Remove-Item .\Tools\$coreToolsFolder -Force -Recurse | Out-Null

##
##Download Configuration Migration
##
nuget install  Microsoft.CrmSdk.XrmTooling.ConfigurationMigration.Wpf -O .\Tools
md .\Tools\ConfigurationMigration  | Out-Null
$configMigFolder = Get-ChildItem ./Tools | Where-Object {$_.Name -match 'Microsoft.CrmSdk.XrmTooling.ConfigurationMigration.Wpf.'}
move .\Tools\$configMigFolder\tools\*.* .\Tools\ConfigurationMigration
Remove-Item .\Tools\$configMigFolder -Force -Recurse | Out-Null

##
##Download Package Deployer 
##
nuget install  Microsoft.CrmSdk.XrmTooling.PackageDeployment.WPF -O .\Tools
md .\Tools\PackageDeployment | Out-Null
$pdFolder = Get-ChildItem ./Tools | Where-Object {$_.Name -match 'Microsoft.CrmSdk.XrmTooling.PackageDeployment.Wpf.'}
move .\Tools\$pdFolder\tools\*.* .\Tools\PackageDeployment
Remove-Item .\Tools\$pdFolder -Force -Recurse | Out-Null

##
##Download Package Deployer PowerShell module
##
nuget install Microsoft.CrmSdk.XrmTooling.PackageDeployment.PowerShell -O .\Tools
$pdPoshFolder = Get-ChildItem ./Tools | Where-Object {$_.Name -match 'Microsoft.CrmSdk.XrmTooling.PackageDeployment.PowerShell.'}
move .\Tools\$pdPoshFolder\tools\*.* .\Tools\PackageDeployment.PowerShell
Remove-Item .\Tools\$pdPoshFolder -Force -Recurse | Out-Null

##
##Download XRM Tooling PowerShell cmdlets
##
nuget install Microsoft.CrmSdk.XrmTooling.CrmConnector.PowerShell -O .\Tools
md .\Tools\XRMToolingPowerShell | Out-Null
$cmdletFolder = Get-ChildItem ./Tools | Where-Object {$_.Name -match 'Microsoft.CrmSdk.XrmTooling.CrmConnector.PowerShell.'}
move .\Tools\$cmdletFolder\tools\*.* .\Tools\XRMToolingPowerShell
Remove-Item .\Tools\$cmdletFolder -Force -Recurse | Out-Null

## 
##Xrm CI Framework 
## 
Uninstall-Module -Name Xrm.Framework.CI.PowerShell.Cmdlets -ErrorAction Ignore
Install-Module -Name Xrm.Framework.CI.PowerShell.Cmdlets -Force