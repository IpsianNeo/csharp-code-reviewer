<#
.SYNOPSIS
    Fetches the Git diff between base and head branch.
.DESCRIPTION
    Fully remote-aware: resolves branches locally and remotely, handles ambiguous refs.
.USAGE
    .\git_diff.ps1 <base-ref> <head-ref>
.PARAMETER Base
    Base branch (default: origin/main)
.PARAMETER Head
    Head branch (default: HEAD)
#>

param(
    [string]$Base = "origin/main",
    [string]$Head = "HEAD"
)

function Resolve-Branch {
    param([string]$Branch)

    # Check remote first
    $remoteRef = git show-ref --verify --quiet "refs/remotes/origin/$Branch"
    if ($LASTEXITCODE -eq 0) { 
        return "origin/$Branch", $true
    }

    # Then local branch
    if (git show-ref --verify --quiet "refs/heads/$Branch") {
        return $Branch, $false
    }

    # Branch not found
    Write-Warning "Branch '$Branch' not found locally or on remote."
    return $null, $false
}

try {
    Write-Host "Resolving base branch '$Base'..."
    $resolvedBase, $baseIsRemote = Resolve-Branch $Base

    Write-Host "Resolving head branch '$Head'..."
    $resolvedHead, $headIsRemote = Resolve-Branch $Head

    if (-not $resolvedBase -or -not $resolvedHead) {
        Write-Host "Skipping diff: one or both branches missing."
        exit 0
    }

    # Fetch from remote if needed
    if ($baseIsRemote) { git fetch origin $Base --quiet }
    if ($headIsRemote) { git fetch origin $Head --quiet }

    Write-Host "Generating diff between $resolvedBase and $resolvedHead..."
    $diff = git diff --unified=0 --no-pager $resolvedBase $resolvedHead

    if ([string]::IsNullOrWhiteSpace($diff)) {
        Write-Host "No differences found."
    } else {
        Write-Output $diff
    }
}
catch {
    Write-Error "Error in git_diff.ps1: $_"
    exit 1
}
