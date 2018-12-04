Param(
        [Parameter(Mandatory=$true, Position=0)]
        [System.IO.DirectoryInfo] $solutiondirectory
)

function Get-ScriptDirectory 
{ 

    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 

} 

#$scriptDir =  (split-Path (Get-ScriptDirectory) -Parent)
$scriptDir =  Get-ScriptDirectory
set-location $scriptDir 
write-host $scriptDir 


. VisualStudioProjectFileFunctions.ps1

$selectedProject = $null
$selectedProject = Get-VsProjectFileInteractive ([System.IO.DirectoryInfo]$solutiondirectory).FullName

Write-Host ""

Write-Host ("1... Check plugin project settings")
Write-Host ("2... Check workflow project settings")
Write-Host ("3... Fix ILMerge reference settings")
Write-Host ("4... Add PreBuild Event to check ILMerge Settings")
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
    1 {Test-VsPluginProjectSettings $selectedProject}
    2 {Test-VsWorkflowProjectSettings $selectedProject}
} 


