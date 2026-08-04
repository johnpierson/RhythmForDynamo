<#
.SYNOPSIS
    Regenerates the node help in docs/nodes from the assemblies in deploy/.

.DESCRIPTION
    One Markdown file per node, named the way Dynamo's documentation browser looks it up. The
    signatures come from the assemblies' metadata and the prose from the XML documentation the
    compiler wrote beside them, so the help cannot drift from the nodes it describes.

    The assemblies are read rather than loaded: RhythmRevit.dll is bound to a Revit API and
    RhythmUI.dll to a Dynamo, and neither needs to be installed for this to run.

    Reads whichever deploy folder you point it at. They are built from one source tree, so the
    node list is the same in all of them; 2027 is the default because it is the newest.

.PARAMETER Assemblies
    The folder holding RhythmCore.dll, RhythmRevit.dll, RhythmUI.dll and their .xml files.

.PARAMETER Output
    Where to write the help. docs/nodes by default.

.PARAMETER Reference
    Where to write the one page that lists the whole library. docs/node-reference.md by default,
    which is the documentation site's reference page. Not part of the shipped package: Dynamo's
    browser matches files to nodes by name and would file it as a node called "node-reference".

.PARAMETER ListUndocumented
    Also print every node whose page carries nothing but its signature.

.EXAMPLE
    ./scripts/generate-docs.ps1
    ./scripts/generate-docs.ps1 -ListUndocumented
#>

[CmdletBinding()]
param(
    [string] $Assemblies,
    [string] $Output,
    [string] $Reference,
    [switch] $ListUndocumented
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $Assemblies) { $Assemblies = Join-Path $repoRoot 'deploy/2027' }
if (-not $Output) { $Output = Join-Path $repoRoot 'docs/nodes' }
if (-not $Reference) { $Reference = Join-Path $repoRoot 'docs/node-reference.md' }

$arguments = @(
    'run', '--project', (Join-Path $repoRoot 'tools/Rhythm.Docs'), '-v', 'quiet', '--'
    '--assemblies', $Assemblies
    '--out', $Output
    '--reference', $Reference
)

if ($ListUndocumented) { $arguments += '--list-undocumented' }

& dotnet @arguments

if ($LASTEXITCODE -ne 0) {
    throw "Node help generation failed with exit code $LASTEXITCODE."
}
