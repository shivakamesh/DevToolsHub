# Renders PNG brand assets (icons + social banner) with System.Drawing.
# Geometry mirrors wwwroot/logo.svg (64x64 design units). Run: powershell -File branding\render-assets.ps1
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$www = Join-Path $root 'wwwroot'

$purple = [System.Drawing.Color]::FromArgb(124, 92, 255)
$cyan   = [System.Drawing.Color]::FromArgb(34, 211, 238)
$pink   = [System.Drawing.Color]::FromArgb(244, 114, 182)
$navy   = [System.Drawing.Color]::FromArgb(11, 16, 32)

function New-Brand([float]$x, [float]$y, [float]$size) {
	$b = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
		(New-Object System.Drawing.PointF($x, $y)),
		(New-Object System.Drawing.PointF(($x + $size), ($y + $size))), $purple, $pink)
	$blend = New-Object System.Drawing.Drawing2D.ColorBlend(3)
	$blend.Colors = @($purple, $cyan, $pink)
	$blend.Positions = @(0.0, 0.55, 1.0)
	$b.InterpolationColors = $blend
	return $b
}

function P([float]$ox, [float]$oy, [float]$s, [float]$x, [float]$y) {
	New-Object System.Drawing.PointF(($ox + $x * $s), ($oy + $y * $s))
}

function Draw-Logo($g, [float]$ox, [float]$oy, [float]$size) {
	$s = $size / 64
	$brand = New-Brand $ox $oy $size

	$outer = [System.Drawing.PointF[]]@((P $ox $oy $s 32 3), (P $ox $oy $s 57 17.5), (P $ox $oy $s 57 46.5),
			   (P $ox $oy $s 32 61), (P $ox $oy $s 7 46.5), (P $ox $oy $s 7 17.5))
	$g.FillPolygon($brand, $outer)

	$shine = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
		(P $ox $oy $s 0 2), (P $ox $oy $s 0 32),
		[System.Drawing.Color]::FromArgb(90, 255, 255, 255), [System.Drawing.Color]::FromArgb(0, 255, 255, 255))
	$g.FillPolygon($shine, $outer)

	$inner = [System.Drawing.PointF[]]@((P $ox $oy $s 32 9.5), (P $ox $oy $s 51.5 20.75), (P $ox $oy $s 51.5 43.25),
			   (P $ox $oy $s 32 54.5), (P $ox $oy $s 12.5 43.25), (P $ox $oy $s 12.5 20.75))
	$g.FillPolygon((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(224, $navy))), $inner)

	$pen = New-Object System.Drawing.Pen($brand, [float](4 * $s))
	$pen.StartCap = 'Round'; $pen.EndCap = 'Round'; $pen.LineJoin = 'Round'
	$g.DrawLines($pen, [System.Drawing.PointF[]]@((P $ox $oy $s 25 23), (P $ox $oy $s 17 32), (P $ox $oy $s 25 41)))
	$g.DrawLines($pen, [System.Drawing.PointF[]]@((P $ox $oy $s 39 23), (P $ox $oy $s 47 32), (P $ox $oy $s 39 41)))

	$star = [System.Drawing.PointF[]]@((P $ox $oy $s 32 24.5), (P $ox $oy $s 34 30), (P $ox $oy $s 39.5 32), (P $ox $oy $s 34 34),
			  (P $ox $oy $s 32 39.5), (P $ox $oy $s 30 34), (P $ox $oy $s 24.5 32), (P $ox $oy $s 30 30))
	$g.FillPolygon([System.Drawing.Brushes]::White, $star)
}

function New-Canvas([int]$w, [int]$h) {
	$bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
	$g = [System.Drawing.Graphics]::FromImage($bmp)
	$g.SmoothingMode = 'AntiAlias'
	$g.InterpolationMode = 'HighQualityBicubic'
	$g.PixelOffsetMode = 'HighQuality'
	$g.TextRenderingHint = 'AntiAliasGridFit'
	return $bmp, $g
}

function Save-Icon([int]$size, [string]$name, [switch]$opaque) {
	$bmp, $g = New-Canvas $size $size
	if ($opaque) { $g.Clear($navy) } else { $g.Clear([System.Drawing.Color]::Transparent) }
	$pad = if ($opaque) { $size * 0.12 } else { $size * 0.02 }
	Draw-Logo $g $pad $pad ($size - 2 * $pad)
	$bmp.Save((Join-Path $www $name), [System.Drawing.Imaging.ImageFormat]::Png)
	$g.Dispose(); $bmp.Dispose()
	Write-Host "Saved $name ($size x $size)"
}

function Add-Orb($g, [float]$cx, [float]$cy, [float]$r, $color, [int]$alpha) {
	$path = New-Object System.Drawing.Drawing2D.GraphicsPath
	$path.AddEllipse(($cx - $r), ($cy - $r), (2 * $r), (2 * $r))
	$brush = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
	$brush.CenterColor = [System.Drawing.Color]::FromArgb($alpha, $color)
	$brush.SurroundColors = [System.Drawing.Color[]]@([System.Drawing.Color]::FromArgb(0, $color))
	$g.FillPath($brush, $path)
}

function Save-OgImage {
	$w = 1200; $h = 630
	$bmp, $g = New-Canvas $w $h
	$g.Clear($navy)
	Add-Orb $g 120 60 420 $purple 150
	Add-Orb $g 1120 640 420 $cyan 130
	Add-Orb $g 780 300 260 $pink 70

	Draw-Logo $g 90 185 260

	$title = New-Object System.Drawing.Font('Segoe UI', 66, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
	$sub   = New-Object System.Drawing.Font('Segoe UI', 28, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
	$chip  = New-Object System.Drawing.Font('Consolas', 20, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
	$fmt = [System.Drawing.StringFormat]::GenericTypographic

	$x = 400; $y = 190
	$g.DrawString('DevTools', $title, [System.Drawing.Brushes]::White, [float]$x, [float]$y, $fmt)
	$devW = $g.MeasureString('DevTools', $title, 2000, $fmt).Width
	$hubW = $g.MeasureString('Hub', $title, 2000, $fmt).Width
	$g.DrawString('Hub', $title, (New-Brand ($x + $devW) $y $hubW), [float]($x + $devW), [float]$y, $fmt)

	$g.DrawString('Free developer tools. 100% in your browser.', $sub,
		(New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(154, 163, 192))), [float]$x, [float]($y + 95), $fmt)

	$cx = [float]$x; $cy = [float]($y + 160)
	$chipFill = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(22, 255, 255, 255))
	$chipPen  = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(40, 255, 255, 255), 1)
	foreach ($label in 'JSON', 'Regex', 'Cron', 'JWT', 'Base64', 'Hash', 'GUID') {
		$tw = $g.MeasureString($label, $chip, 500, $fmt).Width
		$cw = [float]($tw + 32); $ch = [float]40
		$path = New-Object System.Drawing.Drawing2D.GraphicsPath
		$path.AddArc($cx, $cy, $ch, $ch, 90, 180)
		$path.AddArc([float]($cx + $cw - $ch), $cy, $ch, $ch, 270, 180)
		$path.CloseFigure()
		$g.FillPath($chipFill, $path); $g.DrawPath($chipPen, $path)
		$g.DrawString($label, $chip, [System.Drawing.Brushes]::White, [float]($cx + 16), [float]($cy + 9), $fmt)
		$cx += $cw + 10
	}

	$bmp.Save((Join-Path $www 'og-image.png'), [System.Drawing.Imaging.ImageFormat]::Png)
	$g.Dispose(); $bmp.Dispose()
	Write-Host "Saved og-image.png ($w x $h)"
}

Save-Icon 32  'favicon-32.png'
Save-Icon 180 'apple-touch-icon.png' -opaque
Save-Icon 192 'icon-192.png'
Save-Icon 512 'icon-512.png'
Save-Icon 512 'icon-512-maskable.png' -opaque
Save-OgImage
