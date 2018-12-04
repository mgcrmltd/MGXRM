function Get-SettingsJson
{
   Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
   )
   
   #Error if not present
   get-item $jsonfile 
   return (get-content $jsonfile | out-string | convertfrom-json)
}

function Get-JsonObjectAttribute
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 1)]
        [string]$fieldname = $null
    )
    
    return (Get-SettingsJson $jsonfile).$fieldname
}

function Get-XrmSettingConnectionString
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
    )
    
    return Get-JsonObjectAttribute $jsonfile "D365ConnectionString" 
}

function Get-XrmSettingToolsDirectory
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
    )
    
    return Get-JsonObjectAttribute $jsonfile "ToolsDirectory" 
}

function Test-DevToolsInstalled
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 0)]
        [string]$directory = $null
    )

    $tools = "DataMigrationUtility.exe", "PluginRegistration.exe", "CrmSvcUtil.exe", "XRMToolingPowerShell"

    if($jsonfile.Length -gt 0)
    {
        $toolsDir = Get-XrmSettingToolsDirectory $jsonfile
    }
    else
    {
        $toolsDir = $directory
    }
    
    $installDir = get-item $toolsDir -ErrorAction Ignore

    if($installDir -eq $null){
        write-host ("Tools directory {0} does not exist" -f $toolsDir)
        return $false
    }

    foreach($_tool in $tools){
        $exits = Get-ChildItem -Path $toolsDir $_tool -Recurse
        if($exits -eq $null -or $exits.Length -eq 0){
            write-host ("{0} not found" -f $_tool)
            return $false
        }
    }
    
    return $true
}

function Get-XrmSecureOnlineConnectionString{
     <#
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 0)]
        [string]$directory = $null
    )

    $toolingDir = $null

   
    if($jsonfile.Length -gt 0)
    {
        write-host "Register XRM Tooling"
        $scriptDir =  Get-ScriptDirectory

        #. (join-path $scriptDir CommonSettingsFunctions.ps1)
        $toolsDir = Get-XrmSettingToolsDirectory $jsonfile
        $toolingDir = join-path $toolsDir "Tools\XRMToolingPowerShell"
    }
    else
    {
        $toolingDir = $directory
    }
    #>
    $url = (read-host -Prompt "Enter the online URL (e.g. https://mgxrm.crm4.dynamics.com")
    $username = (read-host -Prompt "Username (e.g. me@mycompany.com")
    $password = (Read-Host -Prompt "Password" -AsSecureString)

    $pw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR((($password))))

    $unsecure = ("Url=" + $url + ";Username=" + $username + ";Password=" + $pw + ";authtype=Office365")

    return ($unsecure | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString )
}

function Get-XrmSecureOnPremiseConnectionString{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 0)]
        [string]$directory = $null
    )

    $toolingDir = $null

    if($jsonfile.Length -gt 0)
    {
        write-host "Register XRM Tooling"
        $scriptDir =  Get-ScriptDirectory

        . (join-path $scriptDir CommonSettingsFunctions.ps1)
        $toolsDir = Get-XrmSettingToolsDirectory $jsonfile
        $toolingDir = join-path $toolsDir "Tools\XRMToolingPowerShell"
    }
    else
    {
        $toolingDir = $directory
    }

    $url = (read-host -Prompt "Enter the online URL (e.g. https://mgxrm.crm4.dynamics.com")
    $username = (read-host -Prompt "Username (e.g. me@mycompany.com")
    $password = (Read-Host -Prompt "Password" -AsSecureString)

    $pw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR((($password))))

    $unsecure = ("Url=" + $url + ";Username=" + $username + ";Password=" + $pw + ";authtype=Office365")

    set-location $toolingDir
    .\RegisterXrmTooling.ps1 | out-null

    Write-Host "Testing connection"

    $conn = Get-CrmConnection -ConnectionString $unsecure

    if($conn -eq $null)
    {
        write-host ("Error. Failed to connect to {0} with details supplied. Run this script again" -f $url)
        return $null
    }
    
    write-host ("Connected succesfully")
    return ($unsecure | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString )
}

function Register-XrmTooling{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 1)]
        [string]$directory = $null
    )

    $toolingModule = get-module -name Microsoft.Xrm.Tooling.CrmConnector.PowerShell

    if($toolingModule -eq $null)
    {
        write-host "Microsoft.Xrm.Tooling.CrmConnector.PowerShell already registered"
    }

    $toolingDir = $null

    if($jsonfile.Length -gt 0)
    {
        write-host "Register XRM Tooling"
        $scriptDir =  Get-ScriptDirectory

     #   . (join-path $scriptDir CommonSettingsFunctions.ps1)
        $toolsDir = Get-XrmSettingToolsDirectory $jsonfile
        $toolingDir = join-path $toolsDir "Tools\XRMToolingPowerShell"
    }
    else
    {
        $toolingDir = $directory
    }

    set-location $toolingDir
    .\RegisterXrmTooling.ps1 | out-null
}

function Test-CrmSecuredConnectionString{
    Param(
        [parameter(mandatory=$true, position = 0)]
        [string]$secureconnectionstring,
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 1)]
        [string]$directory = $null
    )

    if([string]::IsNullOrEmpty($jsonfile) -and [string]::IsNullOrEmpty($directory))
    {
        write-host "Either -jsonfile or -directory must be specified to allow registration of CRM modules"
        return $null
    }

    if(![string]::IsNullOrEmpty($jsonfile)) 
    {
        Register-XrmTooling -jsonfile $jsonfile
    }
    else
    {
        Register-XrmTooling -directory $directory
    }

    Write-Host "Testing connection"

    $sec = ($secureconnectionstring | ConvertTo-SecureString)
    $unsecure = ([Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR((($sec)))))
    $conn = Get-CrmConnection -ConnectionString $unsecure

    if($conn -eq $null)
    {
        write-host ("Error. Failed to connect to {0} with details supplied" -f $url)
        return $null
    }
    
    write-host ("Connected succesfully")
    return $conn.ConnectedOrgPublishedEndpoints["WebApplication"];
}

