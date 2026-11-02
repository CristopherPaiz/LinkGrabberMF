# Downloads the pinned WebView2 package from NuGet, checks its hash and extracts the DLLs to lib\.
$ErrorActionPreference = 'Stop'

$Version = '1.0.4191.47'
$Sha256 = 'F492BBF547D0DA329553B6727435B677579B1E9F91CC9E4A1AD029366D5F23D0'
$Url = "https://api.nuget.org/v3-flatcontainer/microsoft.web.webview2/$Version/microsoft.web.webview2.$Version.nupkg"
$Wanted = @{
    'lib/net462/Microsoft.Web.WebView2.Core.dll'     = 'Microsoft.Web.WebView2.Core.dll'
    'lib/net462/Microsoft.Web.WebView2.WinForms.dll' = 'Microsoft.Web.WebView2.WinForms.dll'
    'runtimes/win-x64/native/WebView2Loader.dll'     = 'WebView2Loader.dll'
}

$lib = Join-Path $PSScriptRoot '..\lib'
if (@($Wanted.Values | Where-Object { -not (Test-Path (Join-Path $lib $_)) }).Count -eq 0) {
    Write-Host 'lib\ ya esta listo.'
    return
}

New-Item -ItemType Directory -Force $lib | Out-Null
$package = Join-Path $env:TEMP "webview2-$Version.nupkg"
[Net.ServicePointManager]::SecurityProtocol = 'Tls12'
Write-Host "Descargando WebView2 $Version..."
Invoke-WebRequest -UseBasicParsing -Uri $Url -OutFile $package

if ((Get-FileHash $package -Algorithm SHA256).Hash -ne $Sha256) {
    Remove-Item $package
    throw 'El paquete descargado no coincide con el hash esperado.'
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($package)
try {
    foreach ($entry in $zip.Entries) {
        if ($Wanted.ContainsKey($entry.FullName)) {
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $lib $Wanted[$entry.FullName]), $true)
        }
    }
}
finally {
    $zip.Dispose()
    Remove-Item $package
}
Write-Host 'Dependencias listas en lib\.'
