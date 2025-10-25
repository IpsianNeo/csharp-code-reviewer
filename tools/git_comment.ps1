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

Write-Host "Saving review output to $Out..."
$Content | Out-File -FilePath $Out -Encoding UTF8
Write-Host "Review saved successfully."
