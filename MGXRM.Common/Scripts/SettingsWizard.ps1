param (
    [parameter(position = 0)]
    [string]$jsonfile = $null
)

function Get-ScriptDirectory 
{ 
    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 
} 


$scriptDir =  Get-ScriptDirectory
set-location $scriptDir 

. (join-path $scriptDir CommonSettingsFunctions.ps1)

if([string]::IsNullOrEmpty($jsonfile))
{
    $jsonfile = join-path $scriptDir "commonsettings.json"
}

$toolsDir = (Get-XrmSettingToolsDirectory $jsonfile)

$response = (read-host -prompt ("Specify tools directory. Press enter for default ({0}) or specify" -f $toolsDir))

if([string]::IsNullOrEmpty($response))
{
    $response = $toolsDir    
}

new-item -ItemType directory $response -ErrorAction Ignore

$installed = Test-DevToolsInstalled -directory $response

if($installed -eq $false)
{
    Read-Host -Prompt ("About to install dev tools to {0}. Press ctrl-c to terminate script" -f $response)
    .\InstallXrmTools.ps1 $response
}

write-host "`n`nTools are installed. Will now connect to CRM and securely save the connection string"


$secureString = Get-XrmSecureOnlineConnectionString -directory $response
$connectUrl = Test-CrmSecuredConnectionString -secureconnectionstring $secureString -directory (join-path $response "Tools\XRMToolingPowerShell")
if([string]::IsNullOrEmpty($connectUrl))
{
    write-host "Re-run this script with correct connection details"
    return
}

$obj = New-Object PSObject
Add-Member -InputObject $obj -MemberType NoteProperty -Name ToolsDirectory -Value $response
Add-Member -InputObject $obj -MemberType NoteProperty -Name D365ConnectionString -Value $secureString
Add-Member -InputObject $obj -MemberType NoteProperty -Name Url -Value $connectUrl

ConvertTo-Json -InputObject $obj | set-content (join-path $scriptDir "localcommonsettings.json")


