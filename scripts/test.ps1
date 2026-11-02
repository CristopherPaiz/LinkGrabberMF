# Compiles tests\ against dist\LinkGrab.exe and runs them. Nothing here touches the real JD2 or dist\.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$app = Join-Path $root 'dist\LinkGrab.exe'
$work = Join-Path $env:TEMP 'LinkGrabBuild\tests'
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $app)) { throw 'Falta dist\LinkGrab.exe: compila primero con scripts\build.ps1.' }

if (Test-Path $work) { Remove-Item $work -Recurse -Force }
New-Item -ItemType Directory -Force $work | Out-Null
Copy-Item $app $work

& $csc /nologo /target:exe /platform:x64 /codepage:65001 "/out:$work\LinkGrab.Tests.exe" `
    "/r:$work\LinkGrab.exe" /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll "/recurse:$root\tests\*.cs"
if ($LASTEXITCODE -ne 0) { throw 'Los tests no compilan.' }

$run = Start-Process "$work\LinkGrab.Tests.exe" -NoNewWindow -PassThru
$null = $run.Handle
if (-not $run.WaitForExit(120000)) {
    $run.Kill()
    throw 'Los tests no terminaron en 2 minutos.'
}
if ($run.ExitCode -ne 0) { throw 'Hay tests fallidos.' }
