# Builds dist\LinkGrab.exe with the C# compiler included in Windows; no SDK needed.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$lib = Join-Path $root 'lib'
$obj = Join-Path $root 'obj'
$dist = Join-Path $root 'dist'
$assets = Join-Path $root 'assets'
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

& (Join-Path $PSScriptRoot 'restore.ps1')
New-Item -ItemType Directory -Force $obj, $dist | Out-Null
$icon = Join-Path $obj 'app.ico'
& (Join-Path $PSScriptRoot 'make-icon.ps1') -Output $icon

$dlls = 'Microsoft.Web.WebView2.Core.dll', 'Microsoft.Web.WebView2.WinForms.dll', 'WebView2Loader.dll'
$arguments = @(
    '/nologo', '/optimize+', '/target:winexe', '/platform:x64', '/codepage:65001',
    "/win32icon:$icon", "/win32manifest:$assets\app.manifest", "/out:$dist\LinkGrab.exe",
    '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:System.Web.Extensions.dll',
    "/r:$lib\Microsoft.Web.WebView2.Core.dll", "/r:$lib\Microsoft.Web.WebView2.WinForms.dll",
    "/resource:$assets\rules.ini,rules.ini"
)
$arguments += $dlls | ForEach-Object { "/resource:$lib\$_,$_" }
$arguments += "/recurse:$root\src\*.cs"

& $csc @arguments
if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }

$size = [math]::Round((Get-Item "$dist\LinkGrab.exe").Length / 1KB)
Write-Host "Listo: dist\LinkGrab.exe ($size KB)"
