Param(
        [Parameter(mandatory=$true,Position=0)]
        [string]$projectfile
)

function Get-ScriptDirectory 
{ 
    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 
}

function Initialize-VisualStudioProjectFileFunctions{
    $alreadyinstalled = get-command Test-MergeAssembliesExcluded -ErrorAction Ignore
    if($alreadyinstalled -ne $null){
        return
    }
    
    $scriptDir = Get-ScriptDirectory
    write-host ("scriptdir is {0}" -f $scriptDir)
    . (join-path $scriptDir VisualStudioProjectFileFunctions.ps1) 
}

Initialize-VisualStudioProjectFileFunctions
Test-MergeAssembliesExcluded $projectfile $true