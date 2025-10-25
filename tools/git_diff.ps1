<#
.SYNOPSIS
    Fetches the Git diff between base and head branch.
.DESCRIPTION
    Remote-aware: resolves remote/local branches and generates a proper diff.
.USAGE
    .\git_diff.ps1 <base-ref> <head-ref>
.PARAMETER Base
    The base branch (default: origin/main)
.PARAMETER Head
    The head branch (default: HEAD)
#>

param(
    [string]$Base = "origin/main",
    [string]$Head = "HEAD"
)

function Resolve-Branch {
    param([string]$Branch)
    
    # Remote branch exists?
    if (git show-ref --verify --quiet "refs/remotes/origin/$Branch") {
        $sha = git rev-parse "origin/$Branch"
        return $sha, $true
    }
    # Local branch exists?
    elseif (git show-ref --verify --quiet "refs/heads/$Branch") {
        $sha = git rev-parse "$Branch"
        return $sha, $false
    }
    else {
        Write-Warning "Branch '$Branch' not found locally or on remote."
        return $null, $false
    }
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

    # Fetch only if remote
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
