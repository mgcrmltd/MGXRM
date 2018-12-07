function global:Get-SettingsJson
{
   Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
   )
   
   #Error if not present
   get-item $jsonfile 
   return (get-content $jsonfile | out-string | convertfrom-json)
}

function global:Get-JsonObjectAttribute
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null,
        [parameter(position = 1)]
        [string]$fieldname = $null
    )
    
    return (Get-SettingsJson $jsonfile).$fieldname
}

function global:Get-XrmSettingConnectionString
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
    )
    
    return Get-JsonObjectAttribute $jsonfile "D365ConnectionString" 
}

function global:Get-XrmSettingToolsDirectory
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
    )
    
    return Get-JsonObjectAttribute $jsonfile "ToolsDirectory" 
}

function global:Get-XrmSettingSolution
{
    Param(
        [parameter(position = 0)]
        [string]$jsonfile = $null
    )
    
    return Get-JsonObjectAttribute $jsonfile "Solution" 
}

function global:Test-DevToolsInstalled
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

function global:Get-XrmSecureOnlineConnectionString{
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
    $url = (read-host -Prompt "Enter the online URL (e.g. https://mgxrm.crm4.dynamics.com)")
    $username = (read-host -Prompt "Username (e.g. me@mycompany.com)")
    $password = (Read-Host -Prompt "Password" -AsSecureString)

    $pw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR((($password))))

    $unsecure = ("Url=" + $url + ";Username=" + $username + ";Password=" + $pw + ";authtype=Office365")

    return ($unsecure | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString )
}

function global:Get-XrmSecureOnPremiseConnectionString{
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

    $url = (read-host -Prompt "`nEnter the on-premise URL including port number and org name (e.g. http://myserver:5555/orgname)")
    
    $authType = (Get-NumericResponseFromMenu "Authenticate as logged on user","Specify logon details")

    switch ([int]$authType) 
    {
        1 {
            $unsecure = ("Url=" + $url + ";authtype=AD")
        }
        2 {
            $domain = (read-host -Prompt "Domain")
            $username = (read-host -Prompt "Username")
            $password = (Read-Host -Prompt "Password" -AsSecureString)
            $pw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR((($password))))
            $unsecure = ("Url={0};Domain={1};Username={2};Password={3};authtype=AD" -f $url, $domain, $username, $pw)
        }
    }

    return ($unsecure | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString )
}

function global:Register-XrmTooling{
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

function global:Test-CrmSecuredConnectionString{
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

    $unsecure = Get-UnsecuredString $secureconnectionstring 
    $conn = Get-CrmConnection -ConnectionString $unsecure

    if($conn -eq $null)
    {
        write-host ("Error. Failed to connect to {0} with details supplied" -f $url)
        return $null
    }
    
    write-host ("Connected succesfully")
    return $conn.ConnectedOrgPublishedEndpoints["WebApplication"];
}
function global:Get-UnsecuredString{
    Param(
        [parameter(mandatory=$true, position = 0)]
        [string]$secureString
    )

    $sec = ($secureString | ConvertTo-SecureString)
    return ([Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR((($sec)))))
}

function global:Get-NumericResponseBetween {
    param(
    [parameter(mandatory=$true, position=0)]
    [int]$lowervalue,
    [parameter(mandatory=$true, position=1)]
    [int]$uppervalue
    )

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
        ($gotanumber -and [int]$resp -ge $lowervalue -and [int]$resp -le $uppervalue)
    return $resp
}

function global:Get-NumericResponseFromMenu{
    param(
        [parameter(mandatory=$true, position=0)]
        [string[]]$optionsarray
    )
    [int]$counter = 1
    foreach($opt in $optionsarray)
    {
        write-host ("{0}... {1}" -f $counter, $optionsarray[$counter-1])
        $counter += 1
    }

    return Get-NumericResponseBetween 1 $optionsarray.Count
}

function global:Set-XrmSettings{
    param(
        [parameter(mandatory=$true, position=0)]
        [string]$filename,
        [parameter(mandatory=$true, position=1)]
        [string]$ToolsDirectory,
        [parameter(mandatory=$true, position=2)]
        [string]$D365ConnectionString,
        [parameter(mandatory=$true, position=3)]
        [string]$Url,
        [parameter(mandatory=$true, position=4)]
        [string]$Solution
    )

    $obj = New-Object PSObject
    Add-Member -InputObject $obj -MemberType NoteProperty -Name ToolsDirectory -Value $ToolsDirectory
    Add-Member -InputObject $obj -MemberType NoteProperty -Name D365ConnectionString -Value $D365ConnectionString
    Add-Member -InputObject $obj -MemberType NoteProperty -Name Url -Value $Url
    Add-Member -InputObject $obj -MemberType NoteProperty -Name Solution -Value $Solution

    ConvertTo-Json -InputObject $obj | set-content $filename
    write-host ("Settings saved to {0}" -f $filename)
}