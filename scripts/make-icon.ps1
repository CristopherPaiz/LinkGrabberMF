# Draws the app icon and writes it as an .ico: a small header followed by a PNG.
param([Parameter(Mandatory)][string]$Output)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$size = 256
$bitmap = New-Object Drawing.Bitmap $size, $size
$g = [Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode = 'AntiAlias'
$g.FillEllipse((New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(79, 140, 255))), 8, 8, 240, 240)
$pen = New-Object Drawing.Pen ([Drawing.Color]::White), 34
$pen.StartCap = 'Round'
$pen.EndCap = 'Round'
$g.DrawLine($pen, 128, 64, 128, 170)
$g.DrawLine($pen, 80, 128, 128, 176)
$g.DrawLine($pen, 176, 128, 128, 176)
$g.Dispose()

$png = New-Object IO.MemoryStream
$bitmap.Save($png, [Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()

$headerSize = 22
$writer = New-Object IO.BinaryWriter ([IO.File]::Create($Output))
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]1)
$writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0)
$writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$png.Length); $writer.Write([uint32]$headerSize)
$writer.Write($png.ToArray())
$writer.Dispose()
