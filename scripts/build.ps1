# Replaces the program in dist\ with a single fresh LinkGrab.exe, using the C# compiler included in Windows.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$dist = Join-Path $root 'dist'
$assets = Join-Path $root 'assets'
$work = Join-Path $env:TEMP 'LinkGrabBuild'
$deps = Join-Path $work 'deps'
$packed = Join-Path $work 'packed'
$icon = Join-Path $work 'app.ico'
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

$running = Get-Process LinkGrab -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq "$dist\LinkGrab.exe" }
if ($running) { throw 'Cierra LinkGrab antes de compilar: se esta ejecutando desde dist\.' }

function Compress-Library($source, $target) {
    $reader = [IO.File]::OpenRead($source)
    $writer = [IO.File]::Create($target)
    $zip = New-Object IO.Compression.GZipStream($writer, [IO.Compression.CompressionLevel]::Optimal)
    try { $reader.CopyTo($zip) }
    finally { $zip.Dispose(); $writer.Dispose(); $reader.Dispose() }
}

& (Join-Path $PSScriptRoot 'restore.ps1') -Target $deps
& (Join-Path $PSScriptRoot 'make-icon.ps1') -Output $icon

New-Item -ItemType Directory -Force $packed | Out-Null
$resources = Get-ChildItem "$deps\*.dll" | ForEach-Object {
    $target = Join-Path $packed "$($_.Name).gz"
    Compress-Library $_.FullName $target
    "/resource:$target,$($_.Name).gz"
}

$arguments = @(
    '/nologo', '/optimize+', '/target:winexe', '/platform:x64', '/codepage:65001',
    "/win32icon:$icon", "/win32manifest:$assets\app.manifest", "/out:$work\LinkGrab.exe",
    '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:System.Web.Extensions.dll',
    "/r:$deps\Microsoft.Web.WebView2.Core.dll", "/r:$deps\Microsoft.Web.WebView2.WinForms.dll",
    "/resource:$assets\rules.ini,rules.ini"
) + $resources + "/recurse:$root\src\*.cs"
& $csc @arguments
if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }

# Only now, so a failed compile leaves the previous build in place.
# LinkGrabData holds the user's list and settings, so it survives every build.
New-Item -ItemType Directory -Force $dist | Out-Null
Get-ChildItem $dist -Force | Where-Object { $_.Name -ne 'LinkGrabData' } | Remove-Item -Recurse -Force
Move-Item "$work\LinkGrab.exe" "$dist\LinkGrab.exe"

& (Join-Path $PSScriptRoot 'test.ps1')

$size = [math]::Round((Get-Item "$dist\LinkGrab.exe").Length / 1KB)
Write-Host "Listo: dist\LinkGrab.exe ($size KB)"
