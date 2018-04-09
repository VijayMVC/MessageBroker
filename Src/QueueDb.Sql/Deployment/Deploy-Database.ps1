<# 
.NOTES 
    Name: Publish-Database
    Author: Kelvin Hoover
    Requires: PowerShell v4 or higher

.SYNOPSIS 
    Execute SqlPackage to install or update a database.

.DESCRIPTION 
    This script will export a database model against a target using the SqlPackage.exe utility.

.PARAMETER Environment 
    The environment to deploy to

.PARAMETER Database to deploy (optional)
    Specify the database to deploy, if not specified all the databases will be deployed

.PARAMETER Force
    Forces the database to be recreated

#> 
 
Param ( 
    [ValidateSet("Test")]
    [string] $Environment = 'Test',

    [string] $Database,

    [switch] $Force,

    [switch] $Verbose
)

$ErrorActionPreference = "Stop"

Import-Module $PSScriptRoot\Toolbox.psm1

if( $Verbose )
{
    $VerbosePreference = "Continue"
}

$Environments = @{
    "Test" = [ordered]@{
        "QueueDb" = @{
            "ConnectionString" = "Server=(local);Database=QueueDb_Test;Trusted_Connection=True;"
            "DacpacPath" = "$PSScriptRoot\..\bin\Debug\QueueDb.dacpac"
            "Options" = @()
            }
        }
}

$ErrorActionPreference = "Stop";

function Execute-SqlPackage
{
    Param ( 
        [Parameter(Mandatory=$True)]
        [pscustomobject] $SelectedTarget
    )

    Write-Host "Applying DACPAC $($SelectedTarget.Name)" -ForegroundColor Cyan;

    if( !(test-path -Path $SelectedTarget.Value.DacpacPath) )
    {
        Write-Host "Cannot find dacpac $($SelectedTarget.Value.DacpacPath)" -ForegroundColor Yellow;
        throw "Cannot find dacpac $($SelectedTarget.Value.DacpacPath)";
    }

    $cmds = @();
    $cmds += "/Action:publish";
    $cmds += "/SourceFile:$($SelectedTarget.Value.DacpacPath)";
    $cmds += "/TargetConnectionString:$($SelectedTarget.Value.ConnectionString)";
    $cmds += "/p:DropObjectsNotInSource=true";
    $cmds += "/p:BlockOnPossibleDataLoss=false";

    if( $Force )
    {
        $cmds += "/p:CreateNewDatabase=true";
    }

    foreach($item in $SelectedTarget.Value.Options)
    {
        $cmds += $item;
    }

    Write-Host "Executing SQL Package with the following parameters..." -ForegroundColor Cyan;
    $sqlPackagePath;
    $cmds | Format-Table;

    Invoke-Command -CommandPath $sqlPackagePath -CommandArguments $cmds
}

# Find SqlCmd and SqlPackage.exe
$sqlPackagePath = Get-SqlPackage;
$sqlCmdPath = Get-SqlCmd;

Write-Host "Building for environment $Environment.";
$env = $Environments[$Environment];

foreach($item in $env.GetEnumerator())
{
    if( !$Database -or $Database -eq $item.Name)
    {
        Execute-SqlPackage -SelectedTarget $item;
    }
}

