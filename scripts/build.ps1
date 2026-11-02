# Replaces dist\ with a fresh LinkGrab.exe and lib\, using the C# compiler included in Windows.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$dist = Join-Path $root 'dist'
$assets = Join-Path $root 'assets'
$work = Join-Path $env:TEMP 'LinkGrabBuild'
$deps = Join-Path $work 'deps'
$icon = Join-Path $work 'app.ico'
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

& (Join-Path $PSScriptRoot 'restore.ps1') -Target $deps
& (Join-Path $PSScriptRoot 'make-icon.ps1') -Output $icon

if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory -Force "$dist\lib" | Out-Null

$arguments = @(
    '/nologo', '/optimize+', '/target:winexe', '/platform:x64', '/codepage:65001',
    "/win32icon:$icon", "/win32manifest:$assets\app.manifest", "/out:$dist\LinkGrab.exe",
    '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:System.Web.Extensions.dll',
    "/r:$deps\Microsoft.Web.WebView2.Core.dll", "/r:$deps\Microsoft.Web.WebView2.WinForms.dll",
    "/resource:$assets\rules.ini,rules.ini",
    "/recurse:$root\src\*.cs"
)
& $csc @arguments
if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }

Copy-Item "$deps\*.dll" "$dist\lib"
Write-Host 'Listo: dist\LinkGrab.exe y dist\lib\'