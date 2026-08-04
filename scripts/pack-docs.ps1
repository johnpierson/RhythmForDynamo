<#
.SYNOPSIS
    Copies the generated node help into the package folder Dynamo reads it from.

.DESCRIPTION
    Dynamo's documentation browser looks for a folder named exactly 'doc' beside 'bin' in an
    installed package, scans it for Markdown, and matches each file to a node by its file name.
    Without that folder every Rhythm node shows "no documentation available", which is where this
    package stood until docs/nodes existed.

    Beside each page go its example graph and screenshot, under the same base name. The browser
    finds them by name alone, so nothing here builds a manifest.

    Run scripts/generate-docs.ps1 first; this only copies.

.PARAMETER Source
    The generated help. docs/nodes by default.

.PARAMETER Destination
    The package's doc folder. deploy/Rhythm/doc by default, which sits beside the bin/ the
    extension unpacks into.

.EXAMPLE
    ./scripts/generate-docs.ps1
    ./scripts/pack-docs.ps1
#>

[CmdletBinding()]
param(
    [string] $Source,
    [string] $Destination
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $Source) { $Source = Join-Path $repoRoot 'docs/nodes' }
if (-not $Destination) { $Destination = Join-Path $repoRoot 'deploy/Rhythm/doc' }

if (-not (Test-Path $Source)) {
    throw "No generated help at $Source. Run scripts/generate-docs.ps1 first."
}

# Rebuilt rather than merged. A node withdrawn from the library leaves a stale page behind
# otherwise, and the package would ship help for something nobody can place.
if (Test-Path $Destination) {
    Remove-Item $Destination -Recurse -Force
}

New-Item -ItemType Directory -Path $Destination -Force | Out-Null

$pages = @(Get-ChildItem $Source -Filter *.md | Where-Object { $_.Name -ne 'README.md' })

if ($pages.Count -eq 0) {
    throw "$Source holds no node help. Run scripts/generate-docs.ps1 first."
}

foreach ($page in $pages) {
    Copy-Item $page.FullName $Destination -Force
}

# README.md stays behind: it explains to a contributor how the folder is generated, and handing it
# to Dynamo's browser would file it as a node named "README".
$assets = 0
$skipped = @()

foreach ($asset in Get-ChildItem $Source -File | Where-Object { $_.Extension -ne '.md' }) {
    $name = [IO.Path]::GetFileNameWithoutExtension($asset.Name)
    $node = if ($name.EndsWith('_img')) { $name.Substring(0, $name.Length - 4) } else { $name }

    # Only assets that illustrate a node that still exists. An orphan would otherwise ride along in
    # every package, several megabytes of screenshots of nodes that were renamed years ago.
    if (-not (Test-Path (Join-Path $Source "$node.md"))) {
        $skipped += "$($asset.Name) (no page named $node.md)"
        continue
    }

    # A graph saved before Dynamo 2.0 is XML, and no supported Dynamo opens it. Shipping one gives
    # the help panel an insert button that fails, which is worse than no button.
    if ($asset.Extension -eq '.dyn') {
        $first = (Get-Content $asset.FullName -TotalCount 1).TrimStart([char]0xFEFF, ' ', "`t")
        if (-not $first.StartsWith('{')) {
            $skipped += "$($asset.Name) (pre-2.0 XML graph format)"
            continue
        }
    }

    Copy-Item $asset.FullName $Destination -Force
    $assets++
}

foreach ($note in $skipped) {
    Write-Warning "Not packaged: $note"
}

Write-Host "packed $($pages.Count) node help pages and $assets example assets -> $Destination" -ForegroundColor Green
