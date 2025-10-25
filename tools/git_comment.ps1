<#
.SYNOPSIS
    Saves AI review output to a file.
.USAGE
    .\git_comment.ps1 <output-file-path> "<content>"
#>

param(
    [string]$Out,
    [string]$Content
)

try {
    if ([string]::IsNullOrWhiteSpace($Out)) {
        throw "Output file path not specified."
    }

    Write-Host "Saving review output to '$Out'..."
    $Content | Out-File -FilePath $Out -Encoding UTF8 -Force
    Write-Host "Review saved successfully."
}
catch {
    Write-Error "Error in git_comment.ps1: $_"
    exit 1
}
