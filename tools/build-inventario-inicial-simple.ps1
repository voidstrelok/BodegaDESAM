$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$outputDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\outputs\01a03fc4-13fe-7a91-a529-496b6009af1e'))
$outputPath = Join-Path $outputDir 'inventario_inicial_BodegaDESAM.xlsx'
$stageDir = Join-Path $outputDir ('.simple-build-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $stageDir '_rels') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\_rels') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\worksheets') -Force | Out-Null

function Put([string]$path, [string]$text) {
    [System.IO.File]::WriteAllText($path, $text, [System.Text.UTF8Encoding]::new($false))
}
function Esc([string]$value) {
    if ($null -eq $value) { return '' }
    return [System.Security.SecurityElement]::Escape($value)
}
function Cell([string]$ref, [string]$value) {
    if ($null -eq $value) { return ('<c r="' + $ref + '"/>') }
    return ('<c r="' + $ref + '" t="inlineStr"><is><t>' + (Esc $value) + '</t></is></c>')
}
function Col([int]$number) {
    $result = ''
    while ($number -gt 0) { $number--; $result = [char](65 + ($number % 26)) + $result; $number = [math]::Floor($number / 26) }
    return $result
}

Put (Join-Path $stageDir '[Content_Types].xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>
'@
Put (Join-Path $stageDir '_rels\.rels') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
'@
Put (Join-Path $stageDir 'xl\_rels\workbook.xml.rels') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
</Relationships>
'@
Put (Join-Path $stageDir 'xl\workbook.xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <bookViews><workbookView activeTab="0"/></bookViews>
  <sheets><sheet name="Inventario Inicial" sheetId="1" r:id="rId1"/></sheets>
</workbook>
'@

$headers = @(
    'ID Producto',
    'Producto',
    'ID Marca',
    'Marca',
    'ID Modelo',
    'Modelo',
    'Cantidad',
    'Lote',
    'Fecha Vencimiento',
    'N° Serie(s) (uno por línea)',
    'Observación'
)
$rows = New-Object System.Collections.Generic.List[string]
$headerCells = for ($i=0; $i -lt $headers.Count; $i++) { Cell ((Col ($i + 1)) + '1') $headers[$i] }
$rows.Add(('<row r="1" ht="30" customHeight="1">' + ($headerCells -join '') + '</row>'))
for ($r=2; $r -le 201; $r++) {
    $cells = for ($i=1; $i -le $headers.Count; $i++) { Cell ((Col $i) + $r) $null }
    $rows.Add(('<row r="' + $r + '">' + ($cells -join '') + '</row>'))
}
$cols = '<cols><col min="1" max="1" width="13" customWidth="1"/><col min="2" max="2" width="30" customWidth="1"/><col min="3" max="3" width="11" customWidth="1"/><col min="4" max="4" width="22" customWidth="1"/><col min="5" max="5" width="11" customWidth="1"/><col min="6" max="6" width="24" customWidth="1"/><col min="7" max="7" width="12" customWidth="1"/><col min="8" max="8" width="18" customWidth="1"/><col min="9" max="9" width="18" customWidth="1"/><col min="10" max="10" width="30" customWidth="1"/><col min="11" max="11" width="28" customWidth="1"/></cols>'
$sheet = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0"><pane ySplit="1" topLeftCell="A2" activePane="bottomRight" state="frozen"/><selection pane="bottomRight" activeCell="A2" sqref="A2"/></sheetView></sheetViews>' + $cols + '<sheetData>' + ($rows -join '') + '</sheetData><autoFilter ref="A1:K201"/></worksheet>')
Put (Join-Path $stageDir 'xl\worksheets\sheet1.xml') $sheet

if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }
$fileStream = [System.IO.File]::Open($outputPath, [System.IO.FileMode]::Create)
$archive = [System.IO.Compression.ZipArchive]::new($fileStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)
try {
    foreach ($file in (Get-ChildItem -LiteralPath $stageDir -Recurse -File)) {
        $relative = $file.FullName.Substring($stageDir.Length + 1).Replace('\','/')
        $entry = $archive.CreateEntry($relative, [System.IO.Compression.CompressionLevel]::Optimal)
        $input = [System.IO.File]::OpenRead($file.FullName)
        $entryStream = $entry.Open()
        try { $input.CopyTo($entryStream) } finally { $entryStream.Dispose(); $input.Dispose() }
    }
} finally { $archive.Dispose(); $fileStream.Dispose() }

$zip = [System.IO.Compression.ZipFile]::OpenRead($outputPath)
try {
    foreach ($name in @('[Content_Types].xml','xl/workbook.xml','xl/worksheets/sheet1.xml','xl/_rels/workbook.xml.rels','_rels/.rels')) {
        if (-not $zip.GetEntry($name)) { throw "Falta entrada OOXML: $name" }
    }
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName.EndsWith('.xml') -or $entry.FullName.EndsWith('.rels')) {
            $reader = [IO.StreamReader]::new($entry.Open())
            try { [xml]$reader.ReadToEnd() | Out-Null } finally { $reader.Dispose() }
        }
    }
} finally { $zip.Dispose() }
Write-Output ('CREATED: ' + $outputPath)
