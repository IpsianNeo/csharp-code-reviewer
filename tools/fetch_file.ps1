<#
.SYNOPSIS
    Fetches the full content of a file at a given commit or branch.
.USAGE
    .\fetch_file.ps1 <commit-ref> <file-path>
#>

param(
    [string]$Ref,
    [string]$File
)

Write-Host "Fetching file $File at ref $Ref..."
git show "$Ref`:$File"
