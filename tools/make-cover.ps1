Add-Type -AssemblyName System.Drawing
$w = 1500; $h = 500
$out = Join-Path $PSScriptRoot '..\cover.png'
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'; $g.TextRenderingHint = 'AntiAliasGridFit'
$rect = New-Object System.Drawing.Rectangle 0, 0, $w, $h
$bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, ([System.Drawing.Color]::FromArgb(15,23,42)), ([System.Drawing.Color]::FromArgb(49,46,129)), 20
$g.FillRectangle($bg, $rect)
$gridPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(18,255,255,255)), 1
for ($x = 0; $x -lt $w; $x += 40) { $g.DrawLine($gridPen, $x, 0, $x, $h) }
for ($y = 0; $y -lt $h; $y += 40) { $g.DrawLine($gridPen, 0, $y, $w, $y) }
$accent = [System.Drawing.Color]::FromArgb(56,189,248)
$g.DrawString('{ </> }', (New-Object System.Drawing.Font 'Consolas', 64, ([System.Drawing.FontStyle]::Bold)), (New-Object System.Drawing.SolidBrush $accent), 90, 70)
$g.DrawString("It's All Dev Tools", (New-Object System.Drawing.Font 'Segoe UI', 72, ([System.Drawing.FontStyle]::Bold)), [System.Drawing.Brushes]::White, 80, 170)
$g.DrawString('50+ free developer tools & engineering calculators', (New-Object System.Drawing.Font 'Segoe UI', 26), (New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(203,213,225))), 90, 300)
$g.DrawString('100% in your browser  •  No sign-up  •  Works offline', (New-Object System.Drawing.Font 'Segoe UI Semibold', 20), (New-Object System.Drawing.SolidBrush $accent), 90, 360)
$g.DrawString('www.itsalldevtools.com', (New-Object System.Drawing.Font 'Consolas', 20), [System.Drawing.Brushes]::White, 1150, 440)
$chipFont = New-Object System.Drawing.Font 'Consolas', 16
$chips = 'JSON','Base64','JWT','Regex','SQL','Hash','UUID','QR'
$cy = 60
foreach ($c in $chips) {
  $chipRect = New-Object System.Drawing.RectangleF 1220, $cy, 200, 36
  $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(40,56,189,248))), $chipRect)
  $g.DrawRectangle((New-Object System.Drawing.Pen $accent, 1), 1220, $cy, 200, 36)
  $g.DrawString($c, $chipFont, [System.Drawing.Brushes]::White, 1235, ($cy + 7))
  $cy += 44
}
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "Saved $out"
