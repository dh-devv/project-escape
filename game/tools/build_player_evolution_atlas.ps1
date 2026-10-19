param(
    [string]$AssetRoot = (Join-Path $PSScriptRoot '..\assets\11_Player_Evolution')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceRoot = Join-Path $AssetRoot 'source'
$rowStarts = @(0, 394, 725)
$rowEnds = @(394, 725, 1024)
$cellSize = 256
$limitWidth = 232
$limitHeight = 210

foreach ($phase in @(2, 3)) {
    $inputPath = Join-Path $sourceRoot "phase$phase-generated.png"
    $outputPath = Join-Path $AssetRoot "Player_Evolution_P$phase.png"
    if (-not (Test-Path -LiteralPath $inputPath)) { throw "Missing source sheet: $inputPath" }

    $source = [System.Drawing.Bitmap]::FromFile($inputPath)
    if ($source.Width -ne 1536 -or $source.Height -ne 1024) {
        $source.Dispose()
        throw "Expected 1536x1024 source sheet, got $($source.Width)x$($source.Height): $inputPath"
    }

    $atlas = [System.Drawing.Bitmap]::new(768, 768, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($atlas)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half

    for ($row = 0; $row -lt 3; $row++) {
        for ($column = 0; $column -lt 3; $column++) {
            $minX = $column * 512
            $maxX = $minX + 511
            $minY = $rowStarts[$row]
            $maxY = $rowEnds[$row] - 1
            $left = 1536; $top = 1024; $right = -1; $bottom = -1

            for ($y = $minY; $y -le $maxY; $y++) {
                for ($x = $minX; $x -le $maxX; $x++) {
                    if ($source.GetPixel($x, $y).A -ge 32) {
                        if ($x -lt $left) { $left = $x }
                        if ($x -gt $right) { $right = $x }
                        if ($y -lt $top) { $top = $y }
                        if ($y -gt $bottom) { $bottom = $y }
                    }
                }
            }
            if ($right -lt $left -or $bottom -lt $top) {
                $graphics.Dispose(); $atlas.Dispose(); $source.Dispose()
                throw "Frame ($column,$row) is empty in $inputPath"
            }

            $padding = 3
            $left = [Math]::Max($minX, $left - $padding)
            $right = [Math]::Min($maxX, $right + $padding)
            $top = [Math]::Max($minY, $top - $padding)
            $bottom = [Math]::Min($maxY, $bottom + $padding)
            $width = $right - $left + 1
            $height = $bottom - $top + 1
            $widthScale = [double]$limitWidth / [double]$width
            $heightScale = [double]$limitHeight / [double]$height
            $scale = [Math]::Min($widthScale, $heightScale)
            $drawWidth = [Math]::Max(1, [int][Math]::Round($width * $scale))
            $drawHeight = [Math]::Max(1, [int][Math]::Round($height * $scale))
            $drawX = $column * $cellSize + [int][Math]::Round(($cellSize - $drawWidth) / 2)
            $baseline = if ($row -eq 1 -and $column -eq 1) { 229 } elseif ($row -eq 2 -and $column -eq 0) { 218 } else { 242 }
            $drawY = $row * $cellSize + $baseline - $drawHeight
            $sourceRect = [System.Drawing.Rectangle]::new($left, $top, $width, $height)
            $targetRect = [System.Drawing.Rectangle]::new($drawX, $drawY, $drawWidth, $drawHeight)
            $graphics.DrawImage($source, $targetRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
            Write-Host ("P{0} frame {1}: crop {2}x{3}, output {4}x{5}" -f $phase, ($row * 3 + $column), $width, $height, $drawWidth, $drawHeight)
        }
    }

    $graphics.Dispose()
    $atlas.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $atlas.Dispose()
    $source.Dispose()
    Write-Host "Wrote $outputPath"
}
