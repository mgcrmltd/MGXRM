Param(
        [Parameter(Position=0)]
        [System.IO.DirectoryInfo] $solutiondirectory = $null
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

Initialize-CommonSettingsFunctions

$scriptDir =  Get-ScriptDirectory
set-location $scriptDir 

if([string]::IsNullOrEmpty($solutiondirectory))
{
    $solutiondirectory = ((get-item $scriptDir).Parent.Parent)
}

. (join-path $scriptDir VisualStudioProjectFileFunctions.ps1)

$selectedProject = $null
$selectedProject = Get-VsProjectFileInteractive ([System.IO.DirectoryInfo]$solutiondirectory).FullName

Write-Host ""

$resp = (Get-NumericResponseFromMenu "Check plugin project settings","Check workflow project settings", "Quit")

Write-Host "`n"

switch ([int]$resp) 
{
    1 {Test-VsPluginProjectSettings $selectedProject.FullName}
    2 {Test-VsWorkflowProjectSettings $selectedProject.FullName}
} 

