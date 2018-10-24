function Get-ScriptDirectory 
{ 

    if ($script:MyInvocation.MyCommand.Path) { Split-Path $script:MyInvocation.MyCommand.Path } else { $pwd } 

} 
$scriptDir =  (Get-ScriptDirectory)
set-location (split-Path $scriptDir -Parent) 

Remove-Variable exists -ErrorAction Ignore
$exists = (get-item LocalCommonVariables.ps1 -ErrorAction Ignore)

if($exists -ne $null)
{
    write-host "File already exists. Exiting."
    exit
}

set-content LocalCommonVariables.ps1 "`$devToolsDirectory = ""d:\devtools""`
`$nugetDir = ""c:\devtools\nuget""
`$ = ""xrmmndev02"" 
`$mainDevServer = ""xrmmndev02"" 
[int]`$mainDevPort= 5555 
`$mainDevOrg = ""MainDev"" 
`$mainDevUrl = ""http://"" + `$mainDevServer + "":"" + `$mainDevPort + ""/"" + `$mainDevOrg
"