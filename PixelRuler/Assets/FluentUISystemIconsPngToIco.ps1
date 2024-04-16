#https://icones.js.org/collection/fluent

$pngfiles = Get-ChildItem -Path . -Filter *.png
foreach ($pngfile in $pngfiles)
{
    $outFile = [IO.Path]::ChangeExtension($pngfile.FullName, '.ico')
    $temp = "2" + $pngfile.Name
    magick $pngfile.FullName -fill "rgba(255,255,255,1)" -colorize 100 $temp
    magick $temp -resize 64x64 $outFile
}
