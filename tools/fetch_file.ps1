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

try {
    if (-not (Test-Path $File)) {
        # File may not exist at local HEAD; proceed anyway
        Write-Host "Fetching file '$File' at ref '$Ref'..."
    }

    $content = git show "$Ref`:$File"
    if ([string]::IsNullOrWhiteSpace($content)) {
        Write-Host "Warning: file '$File' is empty at ref '$Ref'."
    }

    Write-Output $content
}
catch {
    Write-Error "Error in fetch_file.ps1: $_"
    exit 1
}
