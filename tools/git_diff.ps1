<#
.SYNOPSIS
    Fetches the Git diff between base and head branch.
.USAGE
    .\git_diff.ps1 <base-ref> <head-ref>
#>

param(
    [string]$Base = "origin/main",
    [string]$Head = "HEAD"
)

Write-Host "Fetching latest commits from origin..."
git fetch origin $Base $Head

Write-Host "Generating diff between $Base and $Head..."
git diff --unified=0 $Base $Head
