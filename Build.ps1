param(
    [string]$Compiler = '',
    [string]$ManagedDirectory = '',
    [string]$ToolsDirectory = (Join-Path $env:TEMP 'FamousCallers-build'),
    [string]$OutputDirectory = '',
    [switch]$CheckOnly
)
$ErrorActionPreference = 'Stop'
$version = '1.1.2'
if (!$OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'dist' }
$portraitIds = @(
    'homer_simpson','marge_simpson','montgomery_burns','peter_griffin','stan_smith',
    'roger','philip_j_fry','turanga_leela','bender','rick_sanchez','spongebob',
    'patrick_star','squidward_tentacles','mr_krabs','shrek','donkey','walter_white',
    'saul_goodman','michael_scott','dwight_schrute'
)
$sources = @('.gitignore','AGENTS.md','README.md','LICENSE','ASSETS.md','Build.ps1','Setup.cs','SpeechInstall.cs','SpeechPatch.cs','RickBurps.cs','Mono.Cecil-LICENSE.txt','setup.manifest','callers.json','voice-design.json','tools/RenderPortraits.cs','tools/GenerateVoices.py')
$sources += @($portraitIds | ForEach-Object { "portraits/$_.png" })
$sources += @($portraitIds | ForEach-Object { "voices/$_.wav" })
function Check-PublishFiles {
    foreach ($relative in $sources) {
        $path = Join-Path $PSScriptRoot $relative
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing public file: $relative" }
        if ([IO.Path]::GetExtension($path) -in @('.cs','.ps1','.py','.md','.json','.manifest')) {
            $contents = [IO.File]::ReadAllText($path)
            if ($contents -match '(?i)(?<![a-z0-9])[a-z]:[\\/]') { throw "Absolute machine path in public source: $relative" }
            if ($contents -match '(?i)(?:ghp_|gho_|github_pat_)[a-z0-9_]{20,}') { throw "Possible credential in public source: $relative" }
        }
    }
    $callers = ConvertFrom-Json -InputObject ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'callers.json')))
    if ($callers.Count -ne 20 -or @($callers.id | Select-Object -Unique).Count -ne 20) { throw 'The catalog needs 20 unique callers.' }
    foreach ($caller in $callers) {
        $slug = $caller.id -replace '^famous_callers_', ''
        if ($slug -notin $portraitIds -or $caller.webcamImage -ne "FamousCallers/$slug.png" -or $caller.voiceId -ne "Callers/FamousCallers/Voices/$slug.wav" -or $caller.firstMessages.Count -ne 3) { throw 'Invalid caller catalog entry.' }
    }
    if (Test-Path -LiteralPath (Join-Path $PSScriptRoot '.git')) {
        $tracked = @(git -C $PSScriptRoot ls-files)
        if ($LASTEXITCODE -ne 0) { throw 'Could not inspect the tracked publication files.' }
        foreach ($relative in $tracked) { if ($relative -notin $sources) { throw "Unexpected tracked file: $relative" } }
    }
    Write-Output 'Publish check passed: only allowlisted product files; no absolute machine paths or recognizable credentials.'
}
Check-PublishFiles
if ($CheckOnly) { return }
if (!$ManagedDirectory -or !(Test-Path -LiteralPath (Join-Path $ManagedDirectory 'PocketTTS.dll'))) { throw 'Supply -ManagedDirectory <game Managed folder> to build the standalone speech runtime.' }
if (!$Compiler) {
    $compilerRoot = Join-Path $ToolsDirectory 'roslyn-4.14.0'
    $Compiler = Join-Path $compilerRoot 'tasks/net472/csc.exe'
    if (!(Test-Path -LiteralPath $Compiler)) {
        New-Item -ItemType Directory -Force -Path $ToolsDirectory | Out-Null
        $archive = Join-Path $ToolsDirectory 'roslyn-4.14.0.zip'
        Invoke-WebRequest 'https://api.nuget.org/v3-flatcontainer/microsoft.net.compilers.toolset/4.14.0/microsoft.net.compilers.toolset.4.14.0.nupkg' -OutFile $archive
        Expand-Archive -LiteralPath $archive -DestinationPath $compilerRoot -Force
    }
}
New-Item -ItemType Directory -Force -Path $OutputDirectory,$ToolsDirectory | Out-Null
$cecilRoot = Join-Path $ToolsDirectory 'mono.cecil-0.11.6'
$cecil = Join-Path $cecilRoot 'lib/net40/Mono.Cecil.dll'
$rocks = Join-Path $cecilRoot 'lib/net40/Mono.Cecil.Rocks.dll'
if (!(Test-Path -LiteralPath $cecil)) {
    $archive = Join-Path $ToolsDirectory 'mono.cecil-0.11.6.zip'
    Invoke-WebRequest 'https://api.nuget.org/v3-flatcontainer/mono.cecil/0.11.6/mono.cecil.0.11.6.nupkg' -OutFile $archive
    if ((Get-FileHash -LiteralPath $archive).Hash -ne 'D2A23832AAA948BA9A01ACC42B5726E34C5F995958F1B30D45C0E7C70B3A72D5') { throw 'Unexpected Cecil package checksum.' }
    Expand-Archive -LiteralPath $archive -DestinationPath $cecilRoot -Force
}
if ((Get-FileHash -LiteralPath $cecil).Hash -ne 'C41BDB9FFD3C5F6E17D2382C1012D73703E035E3F1100245FDD4E08C8DC6EB5B' -or (Get-FileHash -LiteralPath $rocks).Hash -ne '842E09959084EDA733AAB1A5354D7AF79E29594F4D8B91C8792103E5C755ED9B') { throw 'Unexpected Cecil library checksum.' }
$runtime = Join-Path $ToolsDirectory 'FamousCallers.dll'
& $Compiler /nologo /target:library /optimize+ /debug- /deterministic+ "/pathmap:$PSScriptRoot=/_/FamousCallers" "/out:$runtime" "/reference:$(Join-Path $ManagedDirectory 'PocketTTS.dll')" (Join-Path $PSScriptRoot 'RickBurps.cs')
if ($LASTEXITCODE -ne 0) { throw 'Standalone speech runtime compilation failed.' }
$renderer = Join-Path $ToolsDirectory 'RenderPortraits.exe'
& $Compiler /nologo /target:exe /optimize+ /debug- /deterministic+ "/pathmap:$PSScriptRoot=/_/FamousCallers" /reference:System.Drawing.dll "/out:$renderer" (Join-Path $PSScriptRoot 'tools/RenderPortraits.cs')
if ($LASTEXITCODE -ne 0) { throw 'Portrait renderer compilation failed.' }
& $renderer (Join-Path $PSScriptRoot 'portraits')
if ($LASTEXITCODE -ne 0) { throw 'Portrait rendering failed.' }
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Write-Zip([string]$Destination, [string[]]$Paths) {
    $stream = [IO.File]::Create($Destination)
    $zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($relative in $Paths) {
            $entry = $zip.CreateEntry($relative, [IO.Compression.CompressionLevel]::Optimal)
            $inputPath = if ($relative -eq 'runtime/FamousCallers.dll') { $runtime } else { Join-Path $PSScriptRoot $relative }
            $input = [IO.File]::OpenRead($inputPath)
            $output = $entry.Open()
            try { $input.CopyTo($output) } finally { $output.Dispose(); $input.Dispose() }
        }
    } finally { $zip.Dispose(); $stream.Dispose() }
}
$payload = Join-Path $ToolsDirectory 'Payload.zip'
Write-Zip $payload (@('callers.json','runtime/FamousCallers.dll') + @($portraitIds | ForEach-Object { "portraits/$_.png"; "voices/$_.wav" }))
$setup = Join-Path $OutputDirectory 'setup.exe'
& $Compiler /nologo /target:exe /platform:x64 /optimize+ /debug- /deterministic+ "/pathmap:$PSScriptRoot=/_/FamousCallers" "/out:$setup" "/win32manifest:$(Join-Path $PSScriptRoot 'setup.manifest')" /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll "/reference:$cecil" "/reference:$rocks" "/resource:$cecil,FamousCallers.Mono.Cecil" "/resource:$rocks,FamousCallers.Mono.Cecil.Rocks" "/resource:$(Join-Path $PSScriptRoot 'Mono.Cecil-LICENSE.txt'),FamousCallers.Mono.Cecil.License" "/resource:$payload,FamousCallers.Payload" (Join-Path $PSScriptRoot 'Setup.cs') (Join-Path $PSScriptRoot 'SpeechInstall.cs') (Join-Path $PSScriptRoot 'SpeechPatch.cs')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
Check-PublishFiles
$package = Join-Path $OutputDirectory "FamousCallers-v$version-win-x64.zip"
Write-Zip $package $sources
$packageStream = [IO.File]::Open($package, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite)
$packageArchive = [IO.Compression.ZipArchive]::new($packageStream, [IO.Compression.ZipArchiveMode]::Update)
try {
    $entry = $packageArchive.CreateEntry('setup.exe', [IO.Compression.CompressionLevel]::Optimal)
    $entryStream = $entry.Open(); $setupStream = [IO.File]::OpenRead($setup)
    try { $setupStream.CopyTo($entryStream) } finally { $entryStream.Dispose(); $setupStream.Dispose() }
} finally { $packageArchive.Dispose(); $packageStream.Dispose() }
$hashes = @('setup.exe',"FamousCallers-v$version-win-x64.zip") | ForEach-Object {
    (Get-FileHash -LiteralPath (Join-Path $OutputDirectory $_)).Hash.ToLowerInvariant() + '  ' + $_
}
[IO.File]::WriteAllLines((Join-Path $OutputDirectory 'SHA256SUMS.txt'), $hashes)
Write-Output "Built Famous Callers v$version release."
