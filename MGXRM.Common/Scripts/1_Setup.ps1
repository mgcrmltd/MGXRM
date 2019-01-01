param (
    [parameter(position = 0)]
    [bool]$interactive = $true
)

clear-host 

function Get-ScriptDirectory 
{ 

    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 

}
function Set-ScriptDirectoryLocal
{ 
    $sd =  Get-ScriptDirectory
    set-location $sd
    return $sd
} 

$scriptDir = Set-ScriptDirectoryLocal
. (join-path $scriptDir CommonSettingsFunctions.ps1)

if($PSVersionTable.Item("PSVersion").Major -lt 5)
{
    Write-Host ("You must upgrade PowerShell to at least v5. Currently on v{0}" -f $PSVersionTable.Item("PSVersion").Major)
    write-host "Search online for ""Install and Configure WMF 5.1""`n"
    Write-host "Windows Version:"
    Get-WmiObject -Class Win32_OperatingSystem | Format-Table Caption, ServicePackMajorVersion -AutoSize
    exit
}

$sourceControlSettings = Get-Item (join-path $scriptDir "commonsettings.json")
$localSettingsFile = join-path $scriptDir "localcommonsettings.json"
$localSettings = Get-Item ($localSettingsFile) -ErrorAction Ignore

if($localSettings -eq $null)
{
    if($interactive -eq $false)
    {
        
        get-content $sourceControlSettings | Set-Content $localSettingsFile
        write-host "Created localcommonsettings.json from commonsettings.json:"
        Get-Content $localSettingsFile | write-host
        return
    }
    write-host "No local settings found. Will run the settings wizard."
    .\SettingsWizard.ps1
}
else
{
    write-host "Local settings already exist!"
    $obj = (Get-content $localSettingsFile | Out-String | ConvertFrom-Json)
    write-host ("Tools directory:`t{0}" -f $obj.ToolsDirectory)
    write-host ("CRM Instance:`t`t{0}" -f $obj.Url)
    write-host ("Solution:`t`t`t{0}`n" -f $obj.Solution)
    write-host "Checking dev tools installed"
    if((Test-DevToolsInstalled -directory $obj.ToolsDirectory) -eq $false)
    {
        .\InstallXrmTools.ps1 -devToolsDirectory $obj.ToolsDirectory 
    }
}

$scriptDir = Set-ScriptDirectoryLocal

$resp = (Get-NumericResponseFromMenu "Run settings wizard","Use values from source-controlled commonsettings.json", "Re-install dev tools", "Test VS Projects", "Quit")

Write-Host "`n"

switch ([int]$resp) 
{
    1 {.\SettingsWizard.ps1}
    2 {get-content $sourceControlSettings | set-content $localSettingsFile}
    3 {.\InstallXrmTools.ps1 (Get-XrmSettingToolsDirectory $localSettingsFile)}
    4 {.\TestProjects.ps1}
    5 {exit}
}