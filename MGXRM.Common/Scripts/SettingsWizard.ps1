param (
    [parameter(position = 0)]
    [string]$jsonfile = $null,
    [parameter(position = 1)]
    [string]$profilename = "localcommonsettings.json"
)

function Get-ScriptDirectory 
{ 
    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 
} 

function Initialize-CommonSettingsFunctions{
    $alreadyinstalled = get-command Test-NumericResponseFromMenu -ErrorAction Ignore
    if($alreadyinstalled -ne $null){
        return
    }
    $scriptDir = Get-ScriptDirectory
    . (join-path $scriptDir CommonSettingsFunctions.ps1)
}

$scriptDir =  Get-ScriptDirectory


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

new-item -ItemType directory $response -ErrorAction Ignore | Out-Null

$installed = Test-DevToolsInstalled -directory $response

if($installed -eq $false)
{
    Read-Host -Prompt ("About to install dev tools to {0}. Press ctrl-c to terminate script" -f $response)
    .\InstallXrmTools.ps1 $response
}

write-host "`n`nTools are installed. Will now connect to CRM and securely save the connection string"

$crmType = (Get-NumericResponseFromMenu "Connect to CRM online","Connect to CRM on-premise", "Enter a full connection string")

$secureString = ""
switch ([int]$crmType) 
{
    1 {$secureString = Get-XrmSecureOnlineConnectionString -directory $response}
    2 {$secureString = Get-XrmSecureOnPremiseConnectionString -directory $response}
    3 {$connString = (Read-Host -Prompt "Connection string" -AsSecureString)
        $secureString = ($connString | ConvertFrom-SecureString)
    }
}

$connectUrl = Test-CrmSecuredConnectionString -secureconnectionstring $secureString -directory (join-path $response "Tools\XRMToolingPowerShell")
if([string]::IsNullOrEmpty($connectUrl))
{
    write-host "Re-run this script with correct connection details"
    return
}

write-host "Pick an solution to be used for deployment"
$unsecure = Get-UnsecuredString $secureString
$sols = get-xrmentities -ConnectionString $unsecure -EntityName solution -Attribute ismanaged -ConditionOperator Equal -Value $false
$filteredSols = ($sols | Where-Object -Property UniqueName -NE -Value Active | Where-Object -Property UniqueName -NE -Value Basic).UniqueName

$selectedSolution = (Get-NumericResponseFromMenu $filteredSols)

Set-XrmSettings (join-path $scriptDir $profilename) $response $secureString $connectUrl $filteredSols[$selectedSolution-1]
