#Requires -Version 5.1
<#
    resolve-version.ps1
    ---------------------------------------------------------------
    Decides the build version for a CI run from the git ref, and prints it
    as key=value lines for $GITHUB_OUTPUT. Kept out of the workflow so the
    rules can be run and checked locally:

        .\tools\resolve-version.ps1 -Ref refs/tags/v3.1.0-rc.2
        .\tools\resolve-version.ps1 -Ref refs/heads/staging -RunNumber 42

    Adapted from legacycore's tools/resolve-version.ps1. The rules are the
    dev-handbook's (releases/versioning.md):

      refs/tags/vX.Y.Z        kind=release, a release. Must be on main.
      refs/tags/vX.Y.Z-rc.N   kind=release, a release candidate (GitHub
                              prerelease). Must be on staging.
      refs/tags/<other>       throws, so a mistyped tag fails before anything
                              is built or published.
      refs/heads/staging      kind=beta, version <declared>-beta.<run number>,
                              workflow artifacts only. ("beta", not "staging":
                              SemVer sorts prerelease labels alphabetically, and
                              "staging" would outrank "rc".)
      anything else           kind=ci, version <declared>-ci.<run number>,
                              workflow artifacts only.

    <declared> is X.Y.Z from `bundleVersion` in
    ProjectSettings/ProjectSettings.asset (Unity's Player Settings > Version):
    the version staging is working towards. A tag's X.Y.Z must equal it, and it
    must be above the highest release tag, so bump it on staging right after a
    release. CI passes the full version to the Unity build; it never writes
    ProjectSettings.asset back.

    The branch a tag must be on is checked by the workflow, not here: it
    depends on the commit being built, which a local run would not reproduce.

    Outputs: version, prerelease, kind, required_branch
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Ref,
    [int]$RunNumber = 0,
    [string]$RepoDir
)

$ErrorActionPreference = 'Stop'

if (-not $RepoDir) { $RepoDir = Split-Path -Parent $PSScriptRoot }

# Semver core with an optional dotted prerelease. Tags without the v prefix
# (from before the standard) still count when finding the highest release.
$SemVerPattern = '^v?(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$'
# What may be tagged from now on: a release or a numbered release candidate, with the v prefix.
$ReleaseTagPattern = '^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-rc\.[1-9]\d*)?$'

function ConvertTo-SemVer([string]$Text) {
    if ($Text -notmatch $SemVerPattern) { return $null }
    [pscustomobject]@{
        Text       = $Text -replace '^v', ''
        Core       = "$($Matches[1]).$($Matches[2]).$($Matches[3])"
        Major      = [int]$Matches[1]
        Minor      = [int]$Matches[2]
        Patch      = [int]$Matches[3]
        Prerelease = $Matches[4]
    }
}

# SemVer 2.0 precedence. Returns <0, 0 or >0.
function Compare-SemVer($A, $B) {
    foreach ($part in 'Major', 'Minor', 'Patch') {
        if ($A.$part -ne $B.$part) { return $A.$part - $B.$part }
    }
    if (-not $A.Prerelease -and -not $B.Prerelease) { return 0 }
    if (-not $A.Prerelease) { return 1 }       # 1.0.0 > 1.0.0-anything
    if (-not $B.Prerelease) { return -1 }
    $ai = $A.Prerelease.Split('.'); $bi = $B.Prerelease.Split('.')
    for ($i = 0; $i -lt [Math]::Min($ai.Count, $bi.Count); $i++) {
        $an = $ai[$i] -match '^\d+$'; $bn = $bi[$i] -match '^\d+$'
        if ($an -and $bn) { $c = [long]$ai[$i] - [long]$bi[$i] }
        elseif ($an)      { $c = -1 }              # numeric < alphanumeric
        elseif ($bn)      { $c = 1 }
        else              { $c = [string]::CompareOrdinal($ai[$i], $bi[$i]) }
        if ($c -ne 0) { return $c }
    }
    $ai.Count - $bi.Count
}

# The highest release tag (no prerelease), other than $Exclude.
function Get-HighestReleaseTag([string]$Exclude) {
    $tags = @(& git -C $RepoDir tag --list)
    if ($LASTEXITCODE -ne 0) { throw "git tag --list failed in $RepoDir" }
    $highest = $null
    foreach ($tag in $tags) {
        if ($Exclude -and $tag -eq $Exclude) { continue }
        $v = ConvertTo-SemVer $tag
        if (-not $v -or $v.Prerelease) { continue }
        if (-not $highest -or (Compare-SemVer $v $highest) -gt 0) { $highest = $v }
    }
    $highest
}

# The version the repo is working towards (dev-handbook versioning rule 3).
function Get-DeclaredVersion {
    $path = Join-Path $RepoDir 'ProjectSettings/ProjectSettings.asset'
    if (-not (Test-Path $path)) { throw "ProjectSettings.asset not found at $path." }
    $line = Select-String -Path $path -Pattern '^\s*bundleVersion:\s*(.*?)\s*$' | Select-Object -First 1
    if (-not $line) { throw "No bundleVersion in $path. Set Player Settings > Version to the X.Y.Z the repository is working towards." }
    $text = $line.Matches[0].Groups[1].Value
    if ($text -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw "bundleVersion (Player Settings > Version) must be X.Y.Z with no prefix or suffix (got '$text'). CI adds the suffix."
    }
    ConvertTo-SemVer $text
}

$declared = Get-DeclaredVersion
# The declared version must be above the last release, so a released version is
# never built again and a version that was never bumped fails before anything
# is built. The tag being built is left out, or a release tag would fail its
# own check; prereleases are left out, or an rc of the next version would hide
# a stale declared version.
$currentTag  = if ($Ref -match '^refs/tags/(.+)$') { $Matches[1] } else { $null }
$lastRelease = Get-HighestReleaseTag -Exclude $currentTag
if ($lastRelease -and (Compare-SemVer $declared $lastRelease) -le 0) {
    throw ("bundleVersion says $($declared.Text) but $($lastRelease.Text) is already released. " +
           "Bump Player Settings > Version on staging to the next version.")
}

$requiredBranch = ''
if ($Ref -match '^refs/tags/(.+)$') {
    $tag = $Matches[1]
    if ($tag -notmatch $ReleaseTagPattern) {
        throw ("Tag '$tag' is not a release tag. Tag vX.Y.Z on main for a release, or " +
               "vX.Y.Z-rc.N on staging for a release candidate (e.g. v3.1.0, v3.1.0-rc.2). " +
               "Nothing was published. Delete the tag with: git push --delete origin $tag")
    }
    $version = ConvertTo-SemVer $tag
    if ($version.Core -ne $declared.Text) {
        throw ("Tag '$tag' is for $($version.Core) but bundleVersion says $($declared.Text). Nothing was published. " +
               "Set Player Settings > Version on staging first, then tag; or delete the tag with: git push --delete origin $tag")
    }
    $kind           = 'release'
    $requiredBranch = if ($version.Prerelease) { 'staging' } else { 'main' }
}
else {
    $kind = if ($Ref -eq 'refs/heads/staging') { 'beta' } else { 'ci' }
    $version = ConvertTo-SemVer "$($declared.Text)-$kind.$RunNumber"
}

$outputs = [ordered]@{
    version         = $version.Text
    prerelease      = "$([bool]$version.Prerelease)".ToLowerInvariant()
    kind            = $kind
    required_branch = $requiredBranch
}
$outputs.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }
