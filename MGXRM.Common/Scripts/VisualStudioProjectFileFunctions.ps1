function global:Test-MergeAssembliesExcluded
{
    Param(
        [Parameter(Mandatory=$true, Position=0)]
        [System.IO.FileInfo] $projectfile,
        [parameter(Position=1)]
        [bool]$throwexception = $false
    )

    $namespace = "mgxrm"
    $doc = Get-XmlDoc -filename $projectfile.FullName
    $nspacemanager = Get-VsProjectNamespaceManager -xmldoc $doc -namespace $namespace
    [System.Xml.XmlNode]$xmlRootNode = $doc.DocumentElement
    #xpath is complicated by fact that newly added references have no <Private> element. Once the CopyLocal is changed the element appears. Need to test for both 
    $nodes = $xmlRootNode.SelectNodes("/mgxrm:Project/mgxrm:ItemGroup/mgxrm:Reference[(count(mgxrm:HintPath)=1 and count(mgxrm:Private)=0) or (count(mgxrm:HintPath)=1 and count(mgxrm:Private)=1 and count(mgxrm:Private)=1 and mgxrm:Private='True')]/@Include",$nspacemanager)
    if($nodes.Count -gt 0)
    {
      $errorTxt = "This project uses ILMerge and referenced assemblies must have their CopyLocal set to false. "
      $errorTxt += "Update reference settings for:"
        foreach($_ in $nodes)
      {
        $errorTxt += "`n`t- " + ([string]($_.Value)).Substring(0,([string]($_.Value)).IndexOf(","))
      }
      if($throwexception -eq $true)
      {
        throw $errorTxt
      }
      write-host $errorTxt
    }
    else
    {
        write-host "Ok for ILMerge process"
    }
}

function global:Get-VsProjectNamespaceManager
{
    Param
    (
    [Parameter(Mandatory=$true, Position=0)]
    [System.Xml.XmlDocument] $xmldoc,
    [Parameter(Mandatory=$true, Position=0)]
    [string] $namespace
    )

    [System.Xml.XmlNamespaceManager]$namespaceManager=new-object -TypeName System.Xml.XmlNamespaceManager -ArgumentList $xmldoc.NameTable
    $namespaceManager.AddNamespace($namespace,"http://schemas.microsoft.com/developer/msbuild/2003")

    return ,$namespaceManager
    
}

function global:Get-VsProjectFileNodeList
{     
    [OutputType([System.Xml.XmlNodeList])]
    Param
    (
    [Parameter(Mandatory=$true, Position=0)]
    [System.Xml.XmlDocument] $xmldoc,
    [Parameter(Mandatory=$true, Position=1)]
    [System.Xml.XmlNamespaceManager] $namepsacemanager,
    [Parameter(Mandatory=$true, Position=2)]
    [string] $namespace
    )
    
    [System.Xml.XmlNode]$xmlRootNode = $xmldoc.DocumentElement
    $nodes = $xmlRootNode.SelectNodes("/{0}:Project" -f $namespace,$namepsacemanager)
    return [System.Xml.XmlNodeList]$nodes
}

function global:Get-XmlDoc
{     
    [OutputType([System.Xml.XmlDocument])]
    Param
    (
    [Parameter(Mandatory=$true, Position=0)]
    [string] $filename
    )

    $xmlDoc = new-object -TypeName System.Xml.XmlDocument
    $xmlDoc.Load($filename)
    return $xmlDoc

}

function global:Set-VsProjectBuildEvent
{     
    Param
    (
    [Parameter(Mandatory=$true, Position=0)]
    [string] $filename,
    [Parameter(Position=1)]
    [string] $namespace = "mgxrm",
    [Parameter(Mandatory=$true, Position=2)]
    [string] $eventtext,
    [Parameter(Position=3)]
    [bool] $preEvent = $true,
    [Parameter(Position=4)]
    [string] $vsnamespace = "http://schemas.microsoft.com/developer/msbuild/2003"
    )

    $event = "PreBuildEvent"
    if($preEvent -eq $false)
    {
        $event = "PostBuildEvent"
    }

    #Node order creation important so as to avoid empty namepsace strings
    $doc = Get-XmlDoc -filename $filename
    $nspacemanager = Get-VsProjectNamespaceManager -xmldoc $doc -namespace $namespace
    $projectNodes =  Get-VsProjectFileNodeList -xmldoc $doc -namepsacemanager $nspacemanager -namespace $namespace
    $newNode = $doc.CreateNode([System.Xml.XmlNodeType]"Element","PropertyGroup",$vsnamespace)
    $eventNode = $doc.CreateNode([System.Xml.XmlNodeType]"Element",$event,$vsnamespace)
	$eventNode.InnerText = $eventtext
    $newNode.AppendChild($eventNode)
    $projectNodes.AppendChild($newNode)
    $doc.Save($filename)
    write-host "Project file updated"
}

function global:Test-VsProjectScriptInvokedInBuildEvent
{     
    Param
    (
    [Parameter(Mandatory=$true, Position=0)]
    [string] $filename,
    [Parameter(Mandatory=$true, Position=1)]
    [string] $scriptname,
    [Parameter(Position=2)]
    [bool] $preEvent = $true,
    [Parameter(Position=3)]
    [bool] $interactive = $true
    )

    if($scriptname.EndsWith(".ps1"))
    {
        $scriptname = $scriptname.Replace(".ps1","")
    }

    $event = "PreBuildEvent"
    if($preEvent -eq $false)
    {
        $event = "PostBuildEvent"
    }

    $regexText = ("<{0}>.*powershell\.exe.*\$\(SolutionDir\).*\\{1}\.ps1.*\$\(ProjectPath\).*<\/{2}>" -f $event, $scriptname, $event)
    
    $mtchs = (get-item $filename | Select-String -Pattern $regexText)

    if($mtchs -ne $null -and $mtchs.Matches.Count -gt 0){
        return $true
    }
    else{
        if($interactive -eq $true)
        {
            write-host ("No build event found invoking script {0}. Add event?" -f $scriptname)
            Initialize-CommonSettingsFunctions
            $addEvent = (Get-NumericResponseFromMenu "Yes","No")
            switch ([int]$addEvent) 
            {
                1 {Set-VsProjectBuildEvent -filename $filename -eventtext ("powershell.exe $`(SolutionDir)MGXRM.Common\Scripts\{0}.ps1 $`(ProjectPath)" -f $scriptname)}
                2 {}
            }
        }
        return $false
    }


}

function global:Get-VsProjectFileInteractive
{   
    Param(
        [Parameter(Mandatory=$true, Position=0)]
        [string] $path
    )

    $projects = $null
    $counter = 1;
    $projects = (get-childitem -Path $path *.csproj -Recurse)
    try
    {
    
        if($projects.Count -eq 0)
        {
            write-host "No projects found - exiting"
            Exit
        }
        foreach($project in $projects)
        {
            Write-Host ("{0}... {1}" -f $counter.ToString(),$project.Name)
            $counter += 1
        }

    }
    catch
    {
        write-host "Failed to find projects"
        write-host $_.Exception.Message
        Exit
    }

    Write-Host ("{0}... QUIT" -f $counter.ToString())

    $resp = "";
    do
        {
        try
            {
            [int]$resp = read-host -prompt "Select a project"
            $GotANumber = $true
            }
        catch
            {
            $GotANumber = $false
            }
        }
    until
        ($gotanumber -and [int]$resp -le $counter -and [int]$resp -gt 0)

    if([int]$resp -eq $counter)
    {
        write-host "QUIT - Exiting"
        Exit
    }

    return [System.IO.FileInfo]$projects[$resp-1]
}

function global:Test-VsProjectPackage
{
    Param(
        [Parameter(Mandatory=$true, Position=0)]
        [System.IO.FileInfo] $projectfile,
        [Parameter(Mandatory=$true, Position=1)]
        [string] $packagename,
        [Parameter(Position=2)]
        [bool] $showinstallcommand = $true
    )

    $packagefile = get-childitem -Path $projectfile.DirectoryName packages.config
    if($packagefile -eq $null)
    {
        write-host ("Cannot locate packages.config file ")
        return $false
    }

    $xmlcontent = [Xml](get-content $packagefile.FullName)
    $nodes = $xmlcontent.SelectNodes("/packages/package[@id='$packagename']/@version")
    if($nodes -eq $null -or $nodes.Count -eq 0)
    {
        write-host ("MISSING Package:" + $packagename)
        write-host ("Run the following: install-package " + $packagename)
        return $false
    }

    write-host ($packagename + " present as Version: " + $nodes[0].'#text')
    return $true
}

function global:Test-VsPluginProjectSettings
{
    Param(
        [Parameter(Mandatory=$true, Position=0)]
        [System.IO.FileInfo] $projectfile
    )

    Test-VsProjectPackage -projectfile $projectfile -packagename "Microsoft.CrmSdk.CoreAssemblies" | Out-Null
    Test-VsProjectPackage -projectfile $projectfile -packagename "MSBuild.ILMerge.Task" | Out-Null
    Test-MergeAssembliesExcluded $projectfile
    Test-VsProjectScriptInvokedInBuildEvent -filename $projectfile -scriptname BuildEventILMergeCheck.ps1 | out-null
}

function global:Test-VsWorkflowProjectSettings
{
    Param(
        [Parameter(Mandatory=$true, Position=0)]
        [System.IO.FileInfo] $projectfile
    )

    Test-VsProjectPackage -projectfile $projectfile -packagename "Microsoft.CrmSdk.CoreAssemblies" | Out-Null
    Test-VsProjectPackage -projectfile $projectfile -packagename "Microsoft.CrmSdk.Workflow" | Out-Null
    Test-VsProjectPackage -projectfile $projectfile -packagename "MSBuild.ILMerge.Task" | Out-Null
    Test-MergeAssembliesExcluded $projectfile
    Test-VsProjectScriptInvokedInBuildEvent -filename $projectfile -scriptname BuildEventILMergeCheck.ps1 | out-null
}

function global:Get-ScriptDirectory 
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
