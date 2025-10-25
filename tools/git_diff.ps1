<#
.SYNOPSIS
    Fetches the Git diff between base and head branch.
.DESCRIPTION
    This script is remote-aware: uses remote branches if they exist, otherwise falls back to local branches.
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
    # Check if branch exists on remote
    $remoteRef = git show-ref --verify --quiet "refs/remotes/origin/$Branch"
    if ($LASTEXITCODE -eq 0) {
        return "origin/$Branch", $true
    } else {
        # fallback to local
        if (git show-ref --verify --quiet "refs/heads/$Branch") {
            return $Branch, $false
        } else {
            Write-Warning "Branch '$Branch' not found locally or on remote. Skipping diff."
			return $null, $false
        }
    }
}

try {
    Write-Host "Resolving base branch '$Base'..."
    $resolvedBase, $baseIsRemote = Resolve-Branch $Base

    Write-Host "Resolving head branch '$Head'..."
    $resolvedHead, $headIsRemote = Resolve-Branch $Head
	
	if (-not $resolvedHead) {
		Write-Host "Skipping diff: head branch missing."
		exit 0
	}

	
    if ($baseIsRemote) { git fetch origin $Base } 
    if ($headIsRemote) { git fetch origin $Head }

    Write-Host "Generating diff between $resolvedBase and $resolvedHead..."
    $diff = git diff --unified=0 $resolvedBase $resolvedHead

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
