# Opens LinkGrab the way a new user would: empty folder, no saved data. Your real data is never touched.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$app = Join-Path $root 'dist\LinkGrab.exe'
$sandbox = Join-Path $env:TEMP 'LinkGrabSmoke'

if (-not (Test-Path $app)) { throw 'Falta dist\LinkGrab.exe: compila primero con scripts\build.ps1.' }
if (Get-Process LinkGrab -ErrorAction SilentlyContinue) { throw 'Cierra LinkGrab antes de empezar (Salir, desde el area de notificacion).' }

if (Test-Path $sandbox) { Remove-Item $sandbox -Recurse -Force }
New-Item -ItemType Directory $sandbox | Out-Null
Copy-Item $app $sandbox

try {
    Write-Host 'LinkGrab abierto como en un PC nuevo. Sigue la lista "Probar desde cero" del README.'
    Write-Host 'Para terminar: Salir, desde el area de notificacion.'
    $env:LINKGRAB_PROFILE = Join-Path $sandbox 'profile'
    Start-Process (Join-Path $sandbox 'LinkGrab.exe') -Wait
}
finally {
    Remove-Item $sandbox -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Prueba terminada.'
}
