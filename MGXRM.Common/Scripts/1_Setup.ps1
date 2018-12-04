param (
    [parameter(position = 0)]
    [bool]$interactive = $true
)

function Get-ScriptDirectory 
{ 

    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 

} 

clear-host 

if($PSVersionTable.Item("PSVersion").Major -lt 5)
{
    Write-Host ("You must upgrade PowerShell to at least v5. Currently on v{0}" -f $PSVersionTable.Item("PSVersion").Major)
    write-host "Search online for ""Install and Configure WMF 5.1""`n"
    Write-host "Windows Version:"
    Get-WmiObject -Class Win32_OperatingSystem | Format-Table Caption, ServicePackMajorVersion -AutoSize
    exit
}

$scriptDir =  Get-ScriptDirectory
set-location $scriptDir 

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
    write-host "No local settings found."
}
else
{
    write-host "Local settings already exist!"
    $obj = (Get-content $localSettingsFile | Out-String | ConvertFrom-Json)
    write-host ("Tools director:`t`t{0}" -f $obj.ToolsDirectory)
    write-host ("CRM Instance:`t`t{0}`n" -f $obj.Url)
}

Write-Host ("1... Run settings wizard")
Write-Host ("2... Use values from source-controlled commonsettings.json")
Write-Host ("3... QUIT")

$counter += 1
$resp = "";
do
{
    try
    {
        [int]$resp = read-host -prompt "Select an option"
        $GotANumber = $true
    }
    catch
    {
        $GotANumber = $false
    }
}
until
    ($gotanumber -and [int]$resp -ge 1 -and [int]$resp -le 3)

Write-Host "`n"

switch ([int]$resp) 
{
    1 {.\SettingsWizard.ps1}
    2 {get-content $sourceControlSettings | set-content $localSettingsFile}
    3 {exit}
} 

write-host "Saved local settings:"
$obj = (Get-content $localSettingsFile | Out-String | ConvertFrom-Json)
write-host ("Tools directory:`t`t{0}" -f $obj.ToolsDirectory)
write-host ("CRM Instance:`t`t{0}`n" -f $obj.Url)
