$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
Remove-Item Alias:R -ErrorAction SilentlyContinue

$outputDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\outputs\01a03fc4-13fe-7a91-a529-496b6009af1e'))
$outputPath = Join-Path $outputDir 'inventario_inicial_BodegaDESAM.xlsx'
$stageDir = Join-Path $outputDir ('.build-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir '_rels') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\_rels') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\worksheets') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\theme') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'docProps') -Force | Out-Null

function Esc([string]$v) {
    if ($null -eq $v) { return '' }
    return [System.Security.SecurityElement]::Escape($v)
}
function Put([string]$path, [string]$text) {
    [System.IO.File]::WriteAllText($path, $text, [System.Text.UTF8Encoding]::new($false))
}
function C([string]$ref, [string]$value, [int]$style = 0) {
    if ($null -eq $value) { return ('<c r="' + $ref + '" s="' + $style + '"/>') }
    return ('<c r="' + $ref + '" s="' + $style + '" t="inlineStr"><is><t>' + (Esc $value) + '</t></is></c>')
}
function N([string]$ref, [int]$value, [int]$style = 0) {
    return ('<c r="' + $ref + '" s="' + $style + '"><v>' + $value + '</v></c>')
}
function F([string]$ref, [string]$formula, [int]$style = 0) {
    return ('<c r="' + $ref + '" s="' + $style + '"><f>' + (Esc $formula) + '</f></c>')
}
function R([int]$row, [array]$cells, [int]$height = 20) {
    return ('<row r="' + $row + '" ht="' + $height + '" customHeight="1">' + ($cells -join '') + '</row>')
}
function Col([int]$n) {
    $s = ''
    while ($n -gt 0) { $n--; $s = [char](65 + ($n % 26)) + $s; $n = [math]::Floor($n / 26) }
    return $s
}
function BlankRow([int]$row, [int]$lastCol, [int]$style = 0, [int]$height = 20) {
    $cells = 1..$lastCol | ForEach-Object { C ((Col $_) + $row) $null $style }
    return R $row $cells $height
}
function TitleRow([int]$row, [int]$lastCol, [string]$text, [int]$style, [int]$height) {
    $cells = @((C (Col 1) + $row $text $style))
    for ($i=2; $i -le $lastCol; $i++) { $cells += C ((Col $i) + $row) $null $style }
    return R $row $cells $height
}

$styles = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <numFmts count="1"><numFmt numFmtId="164" formatCode="yyyy-mm-dd"/></numFmts>
  <fonts count="5">
    <font><sz val="10"/><color theme="1"/><name val="Aptos"/></font>
    <font><b/><sz val="16"/><color rgb="FFFFFFFF"/><name val="Aptos Display"/></font>
    <font><b/><sz val="10"/><color rgb="FFFFFFFF"/><name val="Aptos"/></font>
    <font><b/><sz val="10"/><color rgb="FF16324F"/><name val="Aptos"/></font>
    <font><i/><sz val="10"/><color rgb="FF52606D"/><name val="Aptos"/></font>
  </fonts>
  <fills count="9">
    <fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF16324F"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF0F766E"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFE9F3F2"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFFFF4CC"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFF5F7FA"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFE8F5E9"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFFDECEC"/><bgColor indexed="64"/></patternFill></fill>
  </fills>
  <borders count="4">
    <border><left/><right/><top/><bottom/><diagonal/></border>
    <border><left style="thin"><color rgb="FFD7DEE8"/></left><right style="thin"><color rgb="FFD7DEE8"/></right><top style="thin"><color rgb="FFD7DEE8"/></top><bottom style="thin"><color rgb="FFD7DEE8"/></bottom><diagonal/></border>
    <border><left/><right/><top/><bottom style="thin"><color rgb="FFB8C4D1"/></bottom><diagonal/></border>
    <border><left style="medium"><color rgb="FF0F766E"/></left><right style="medium"><color rgb="FF0F766E"/></right><top style="medium"><color rgb="FF0F766E"/></top><bottom style="medium"><color rgb="FF0F766E"/></bottom><diagonal/></border>
  </borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="15">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" applyAlignment="1"><alignment vertical="center"/></xf>
    <xf numFmtId="0" fontId="1" fillId="2" borderId="0" applyAlignment="1"><alignment vertical="center"/></xf>
    <xf numFmtId="0" fontId="2" fillId="3" borderId="0" applyAlignment="1"><alignment vertical="center"/></xf>
    <xf numFmtId="0" fontId="3" fillId="4" borderId="2" applyAlignment="1"><alignment vertical="center"/></xf>
    <xf numFmtId="0" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="3" fillId="6" borderId="0" applyAlignment="1"><alignment vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="6" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="right"/></xf>
    <xf numFmtId="164" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="3" fillId="7" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="left" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="6" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="right"/></xf>
    <xf numFmtId="0" fontId="0" fillId="7" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="right"/></xf>
    <xf numFmtId="0" fontId="3" fillId="4" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="left"/></xf>
    <xf numFmtId="0" fontId="4" fillId="6" borderId="0" applyAlignment="1"><alignment vertical="top" horizontal="left" wrapText="1"/></xf>
  </cellXfs>
  <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
  <dxfs count="3">
    <dxf><font><b/><color rgb="FF9B1C1C"/></font><fill><patternFill patternType="solid"><fgColor rgb="FFFDECEC"/></patternFill></fill></dxf>
    <dxf><font><b/><color rgb="FF8A5A00"/></font><fill><patternFill patternType="solid"><fgColor rgb="FFFFF4CC"/></patternFill></fill></dxf>
    <dxf><font><b/><color rgb="FF1B5E20"/></font><fill><patternFill patternType="solid"><fgColor rgb="FFE8F5E9"/></patternFill></fill></dxf>
  </dxfs>
</styleSheet>
'@
Put (Join-Path $stageDir 'xl\styles.xml') $styles

$ct = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
  <Override PartName="/xl/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
  <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet3.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet4.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>
'@
Put (Join-Path $stageDir '[Content_Types].xml') $ct
Put (Join-Path $stageDir '_rels\.rels') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
</Relationships>
'@
Put (Join-Path $stageDir 'xl\_rels\workbook.xml.rels') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml"/>
  <Relationship Id="rId4" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet4.xml"/>
  <Relationship Id="rId5" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
  <Relationship Id="rId6" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme" Target="theme/theme1.xml"/>
</Relationships>
'@
Put (Join-Path $stageDir 'xl\workbook.xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <bookViews><workbookView xWindow="120" yWindow="120" windowWidth="24000" windowHeight="12000" activeTab="0"/></bookViews>
  <sheets><sheet name="Inventario Inicial" sheetId="1" r:id="rId1"/><sheet name="Resumen" sheetId="2" r:id="rId2"/><sheet name="Catálogos" sheetId="3" r:id="rId3"/><sheet name="Instrucciones" sheetId="4" r:id="rId4"/></sheets>
  <calcPr calcId="191029" calcMode="auto" fullCalcOnLoad="1" forceFullCalc="1"/>
</workbook>
'@
Put (Join-Path $stageDir 'xl\theme\theme1.xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<a:theme xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" name="BodegaDESAM">
  <a:themeElements>
    <a:clrScheme name="BodegaDESAM"><a:dk1><a:sysClr val="windowText" lastClr="000000"/></a:dk1><a:lt1><a:sysClr val="window" lastClr="FFFFFF"/></a:lt1><a:dk2><a:srgbClr val="16324F"/></a:dk2><a:lt2><a:srgbClr val="F5F7FA"/></a:lt2><a:accent1><a:srgbClr val="0F766E"/></a:accent1><a:accent2><a:srgbClr val="E9F3F2"/></a:accent2><a:accent3><a:srgbClr val="FFF4CC"/></a:accent3><a:accent4><a:srgbClr val="E8F5E9"/></a:accent4><a:accent5><a:srgbClr val="FDECEC"/></a:accent5><a:accent6><a:srgbClr val="52606D"/></a:accent6><a:hlink><a:srgbClr val="0563C1"/></a:hlink><a:folHlink><a:srgbClr val="954F72"/></a:folHlink></a:clrScheme>
    <a:fontScheme name="BodegaDESAM"><a:majorFont><a:latin typeface="Aptos Display"/><a:ea typeface=""/><a:cs typeface=""/></a:majorFont><a:minorFont><a:latin typeface="Aptos"/><a:ea typeface=""/><a:cs typeface=""/></a:minorFont></a:fontScheme>
    <a:fmtScheme name="BodegaDESAM"><a:fillStyleLst><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:gradFill rotWithShape="1"><a:gsLst><a:gs pos="0"><a:schemeClr val="phClr"/></a:gs><a:gs pos="100000"><a:schemeClr val="phClr"/></a:gs></a:gsLst><a:lin ang="5400000" scaled="0"/></a:gradFill><a:gradFill rotWithShape="1"><a:gsLst><a:gs pos="0"><a:schemeClr val="phClr"/></a:gs><a:gs pos="100000"><a:schemeClr val="phClr"/></a:gs></a:gsLst><a:lin ang="5400000" scaled="0"/></a:gradFill></a:fillStyleLst><a:lnStyleLst><a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:prstDash val="solid"/></a:ln><a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:prstDash val="solid"/></a:ln><a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:prstDash val="solid"/></a:ln></a:lnStyleLst><a:effectStyleLst><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst/></a:effectStyle></a:effectStyleLst><a:bgFillStyleLst><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:gradFill rotWithShape="1"><a:gsLst><a:gs pos="0"><a:schemeClr val="phClr"/></a:gs><a:gs pos="100000"><a:schemeClr val="phClr"/></a:gs></a:gsLst><a:lin ang="5400000" scaled="0"/></a:gradFill><a:gradFill rotWithShape="1"><a:gsLst><a:gs pos="0"><a:schemeClr val="phClr"/></a:gs><a:gs pos="100000"><a:schemeClr val="phClr"/></a:gsLst><a:lin ang="5400000" scaled="0"/></a:gradFill></a:bgFillStyleLst></a:fmtScheme>
  </a:themeElements>
</a:theme>
'@
Put (Join-Path $stageDir 'xl\theme\theme1.xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<a:theme xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" name="BodegaDESAM">
  <a:themeElements>
    <a:clrScheme name="BodegaDESAM"><a:dk1><a:sysClr val="windowText" lastClr="000000"/></a:dk1><a:lt1><a:sysClr val="window" lastClr="FFFFFF"/></a:lt1><a:dk2><a:srgbClr val="16324F"/></a:dk2><a:lt2><a:srgbClr val="F5F7FA"/></a:lt2><a:accent1><a:srgbClr val="0F766E"/></a:accent1><a:accent2><a:srgbClr val="E9F3F2"/></a:accent2><a:accent3><a:srgbClr val="FFF4CC"/></a:accent3><a:accent4><a:srgbClr val="E8F5E9"/></a:accent4><a:accent5><a:srgbClr val="FDECEC"/></a:accent5><a:accent6><a:srgbClr val="52606D"/></a:accent6><a:hlink><a:srgbClr val="0563C1"/></a:hlink><a:folHlink><a:srgbClr val="954F72"/></a:folHlink></a:clrScheme>
    <a:fontScheme name="BodegaDESAM"><a:majorFont><a:latin typeface="Aptos Display"/><a:ea typeface=""/><a:cs typeface=""/></a:majorFont><a:minorFont><a:latin typeface="Aptos"/><a:ea typeface=""/><a:cs typeface=""/></a:minorFont></a:fontScheme>
    <a:fmtScheme name="BodegaDESAM"><a:fillStyleLst><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:fillStyleLst><a:lnStyleLst><a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:prstDash val="solid"/></a:ln><a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:prstDash val="solid"/></a:ln><a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:prstDash val="solid"/></a:ln></a:lnStyleLst><a:effectStyleLst><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst/></a:effectStyle></a:effectStyleLst><a:bgFillStyleLst><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:solidFill><a:schemeClr val="phClr"/></a:solidFill><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:bgFillStyleLst></a:fmtScheme>
  </a:themeElements>
</a:theme>
'@
Put (Join-Path $stageDir 'docProps\core.xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><dc:title>Inventario inicial BodegaDESAM</dc:title><dc:creator>BodegaDESAM</dc:creator><cp:lastModifiedBy>BodegaDESAM</cp:lastModifiedBy><dcterms:created xsi:type="dcterms:W3CDTF">2026-08-27T12:00:00Z</dcterms:created></cp:coreProperties>
'@
Put (Join-Path $stageDir 'docProps\app.xml') @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"><Application>Microsoft Excel Compatible</Application><DocSecurity>0</DocSecurity><ScaleCrop>false</ScaleCrop><HeadingPairs><vt:vector size="2" baseType="variant"><vt:variant><vt:lpstr>Worksheets</vt:lpstr></vt:variant><vt:variant><vt:i4>4</vt:i4></vt:variant></vt:vector></HeadingPairs><TitlesOfParts><vt:vector size="4" baseType="lpstr"><vt:lpstr>Inventario Inicial</vt:lpstr><vt:lpstr>Resumen</vt:lpstr><vt:lpstr>Catálogos</vt:lpstr><vt:lpstr>Instrucciones</vt:lpstr></vt:vector></TitlesOfParts><Company>BodegaDESAM</Company><LinksUpToDate>false</LinksUpToDate><SharedDoc>false</SharedDoc><HyperlinksChanged>false</HyperlinksChanged><AppVersion>16.0000</AppVersion></Properties>
'@

$rows = New-Object System.Collections.Generic.List[string]
$rows.Add((TitleRow 1 14 'Planilla de Inventario Inicial — BodegaDESAM' 1 30))
$rows.Add((TitleRow 2 14 'Levantamiento físico y preparación del ajuste inicial de existencias' 2 22))
$rows.Add((BlankRow 3 14 0 8))
$rows.Add((R 4 @((C 'A4' 'Bodega' 3),(C 'B4' $null 3),(C 'C4' 'Bodega Central DESAM' 4),(C 'D4' $null 4),(C 'E4' $null 4),(C 'F4' $null 4),(C 'G4' 'Fecha ajuste' 3),(C 'H4' $null 3),(C 'I4' $null 8),(C 'J4' $null 8),(C 'K4' 'Motivo' 3),(C 'L4' $null 3),(C 'M4' 'Inventario inicial' 4),(C 'N4' $null 4)) 22))
$meta = @((C 'A5' 'Observación general' 3),(C 'B5' $null 3))
for ($i=3; $i -le 14; $i++) { $meta += C ((Col $i) + '5') $null 4 }
$rows.Add((R 5 $meta 24))
$rows.Add((TitleRow 6 14 'Celdas amarillas = completar. Usa Tipo de ajuste = Aumento para el inventario inicial. No borres las fórmulas de Control.' 14 26))
$rows.Add((BlankRow 7 14 0 8))
$headers = @('N°','ID Producto','Producto','ID Marca','Marca','ID Modelo','Modelo','Tipo de ajuste','Cantidad','Lote (opcional)','Fecha vencimiento','N° Serie(s) (uno por línea)','Observación ítem','Control')
$headerCells = for ($i=0; $i -lt $headers.Count; $i++) { C ((Col ($i+1)) + '8') $headers[$i] 2 }
$rows.Add((R 8 $headerCells 42))
for ($r=9; $r -le 108; $r++) {
    $cells = @((N ("A$r") ($r - 8) 6),(C "B$r" $null 7),(C "C$r" $null 10),(C "D$r" $null 7),(C "E$r" $null 10),(C "F$r" $null 7),(C "G$r" $null 10),(C "H$r" 'Aumento' 4),(C "I$r" $null 7),(C "J$r" $null 10),(C "K$r" $null 8),(C "L$r" $null 10),(C "M$r" $null 10))
    $formula = ('IF(COUNTA(B{0}:G{0},I{0}:M{0})=0,"",IF(OR(B{0}="",C{0}="",D{0}="",E{0}="",I{0}="",I{0}<1,LEN(J{0})>50,LEN(M{0})>200),"REVISAR",IF(H{0}<>"Aumento","REVISAR",IF(COUNTIFS($B$9:$B$108,B{0},$D$9:$D$108,D{0},$F$9:$F$108,F{0},$J$9:$J$108,J{0})>1,"DUPLICADO","OK"))))' -f $r)
    $cells += F "N$r" $formula 9
    $rows.Add((R $r $cells 30))
}
$cols1 = '<cols><col min="1" max="1" width="6" customWidth="1"/><col min="2" max="2" width="12" customWidth="1"/><col min="3" max="3" width="25" customWidth="1"/><col min="4" max="4" width="12" customWidth="1"/><col min="5" max="5" width="20" customWidth="1"/><col min="6" max="6" width="12" customWidth="1"/><col min="7" max="7" width="22" customWidth="1"/><col min="8" max="8" width="17" customWidth="1"/><col min="9" max="9" width="11" customWidth="1"/><col min="10" max="10" width="18" customWidth="1"/><col min="11" max="11" width="16" customWidth="1"/><col min="12" max="12" width="24" customWidth="1"/><col min="13" max="13" width="23" customWidth="1"/><col min="14" max="14" width="14" customWidth="1"/></cols>'
$extra1 = @'
<autoFilter ref="A8:N108"/>
<conditionalFormatting sqref="N9:N108"><cfRule type="expression" dxfId="0" priority="1"><formula>N9="REVISAR"</formula></cfRule></conditionalFormatting>
<conditionalFormatting sqref="N9:N108"><cfRule type="expression" dxfId="1" priority="2"><formula>N9="DUPLICADO"</formula></cfRule></conditionalFormatting>
<conditionalFormatting sqref="N9:N108"><cfRule type="expression" dxfId="2" priority="3"><formula>N9="OK"</formula></cfRule></conditionalFormatting>
<dataValidations count="3">
<dataValidation type="list" allowBlank="1" showErrorMessage="1" showInputMessage="1" promptTitle="Tipo de ajuste" prompt="Para inventario inicial usa Aumento." errorTitle="Valor no válido" error="Usa Aumento o Disminución." sqref="H9:H108"><formula1>"Aumento,Disminución"</formula1></dataValidation>
<dataValidation type="whole" operator="greaterThan" allowBlank="1" showErrorMessage="1" errorTitle="ID no válido" error="El ID debe ser mayor que 0." sqref="B9:B108 D9:D108 F9:F108"><formula1>0</formula1></dataValidation>
<dataValidation type="whole" operator="greaterThan" allowBlank="1" showErrorMessage="1" errorTitle="Cantidad no válida" error="La cantidad debe ser mayor que 0." sqref="I9:I108"><formula1>0</formula1></dataValidation>
</dataValidations>
'@
$sheet1 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"><pane xSplit="2" ySplit="8" topLeftCell="C9" activePane="bottomRight" state="frozen"/><selection pane="bottomRight" activeCell="C9" sqref="C9"/></sheetView></sheetViews>' + $cols1 + '<sheetData>' + ($rows -join '') + '</sheetData>' + $extra1 + '<mergeCells count="5"><mergeCell ref="A1:N1"/><mergeCell ref="A2:N2"/><mergeCell ref="C4:F4"/><mergeCell ref="M4:N4"/><mergeCell ref="C5:N5"/></mergeCells></worksheet>')
Put (Join-Path $stageDir 'xl\worksheets\sheet1.xml') $sheet1

$s2 = New-Object System.Collections.Generic.List[string]
$s2.Add((TitleRow 1 8 'Resumen de control — Inventario inicial' 1 30))
$s2.Add((TitleRow 2 8 'Revisa esta hoja antes de transcribir el ajuste en la plataforma.' 2 22))
$s2.Add((BlankRow 3 8 0 8))
$s2.Add((R 4 @((C 'A4' 'Estado general' 3),(F 'B4' 'IF(B8=0,"LISTA PARA CARGA","REVISAR FILAS")' 12),(C 'C4' $null 0),(C 'D4' $null 0),(C 'E4' 'Criterio' 3),(C 'F4' 'LISTA PARA CARGA = no hay filas con errores' 13),(C 'G4' $null 13),(C 'H4' $null 13)) 24))
$s2.Add((BlankRow 5 8 0 8))
$s2.Add((R 6 @((C 'A6' 'Total de filas utilizadas' 5),(F 'B6' 'COUNTIF(''Inventario Inicial''!$N$9:$N$108,"OK")+COUNTIF(''Inventario Inicial''!$N$9:$N$108,"REVISAR")+COUNTIF(''Inventario Inicial''!$N$9:$N$108,"DUPLICADO")' 11),(C 'D6' 'Aumentos = unidades a ingresar' 5),(F 'E6' 'SUMIF(''Inventario Inicial''!$H$9:$H$108,"Aumento",''Inventario Inicial''!$I$9:$I$108)' 12)) 22))
$s2.Add((R 7 @((C 'A7' 'Filas OK' 5),(F 'B7' 'COUNTIF(''Inventario Inicial''!$N$9:$N$108,"OK")' 12),(C 'D7' 'Disminuciones' 5),(F 'E7' 'SUMIF(''Inventario Inicial''!$H$9:$H$108,"Disminución",''Inventario Inicial''!$I$9:$I$108)' 12)) 22))
$s2.Add((R 8 @((C 'A8' 'Filas por revisar' 5),(F 'B8' 'COUNTIF(''Inventario Inicial''!$N$9:$N$108,"REVISAR")+COUNTIF(''Inventario Inicial''!$N$9:$N$108,"DUPLICADO")' 12),(C 'D8' 'Neto inicial' 5),(F 'E8' 'E6-E7' 12)) 22))
$s2.Add((R 9 @((C 'A9' 'Nota' 3),(C 'B9' 'El inventario inicial normalmente debe tener 0 disminuciones. Si aparece alguna, revise el tipo de ajuste.' 14),(C 'C9' $null 14),(C 'D9' $null 14),(C 'E9' $null 14),(C 'F9' $null 14),(C 'G9' $null 14),(C 'H9' $null 14)) 34))
$s2.Add((R 10 @((C 'A10' 'Control aplicado' 3),(C 'B10' 'Producto, marca y cantidad obligatorios; lote máximo 50; observación máxima 200; tipo inicial = Aumento; duplicados por producto/marca/modelo/lote.' 14),(C 'C10' $null 14),(C 'D10' $null 14),(C 'E10' $null 14),(C 'F10' $null 14),(C 'G10' $null 14),(C 'H10' $null 14)) 44))
$cols2 = '<cols><col min="1" max="1" width="27" customWidth="1"/><col min="2" max="2" width="18" customWidth="1"/><col min="3" max="3" width="3" customWidth="1"/><col min="4" max="4" width="27" customWidth="1"/><col min="5" max="5" width="18" customWidth="1"/><col min="6" max="8" width="22" customWidth="1"/></cols>'
$sheet2 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"/></sheetViews>' + $cols2 + '<sheetData>' + ($s2 -join '') + '</sheetData><mergeCells count="5"><mergeCell ref="A1:H1"/><mergeCell ref="A2:H2"/><mergeCell ref="F4:H4"/><mergeCell ref="B9:H9"/><mergeCell ref="B10:H10"/></mergeCells></worksheet>')
Put (Join-Path $stageDir 'xl\worksheets\sheet2.xml') $sheet2

$s3 = New-Object System.Collections.Generic.List[string]
$s3.Add((TitleRow 1 12 'Catálogos de apoyo' 1 30))
$s3.Add((TitleRow 2 12 'Completa o copia aquí los nombres e IDs que ves en la plataforma para evitar errores.' 2 22))
$s3.Add((R 3 @((C 'A3' 'ID Producto' 2),(C 'B3' 'Producto' 2),(C 'C3' $null 2),(C 'D3' 'ID Marca' 2),(C 'E3' 'Marca' 2),(C 'F3' $null 2),(C 'G3' 'ID Modelo' 2),(C 'H3' 'ID Marca' 2),(C 'I3' 'Modelo' 2),(C 'J3' $null 2),(C 'K3' 'ID Bodega' 2),(C 'L3' 'Bodega' 2)) 32))
$brands = @('Genérica','3M','Kimberly-Clark')
for ($r=4; $r -le 23; $r++) {
    $brand = if ($r -le 6) { $brands[$r-4] } else { $null }
    $s3.Add((R $r @((C "A$r" $null 7),(C "B$r" $null 10),(C "C$r" $null 0),(C "D$r" $null 7),(C "E$r" $brand 10),(C "F$r" $null 0),(C "G$r" $null 7),(C "H$r" $null 7),(C "I$r" $null 10),(C "J$r" $null 0),(C "K$r" $null 7),(C "L$r" $(if ($r -eq 4) { 'Bodega Central DESAM' } else { $null }) 10)) 22))
}
$cols3 = '<cols><col min="1" max="1" width="13" customWidth="1"/><col min="2" max="2" width="28" customWidth="1"/><col min="3" max="3" width="3" customWidth="1"/><col min="4" max="4" width="13" customWidth="1"/><col min="5" max="5" width="22" customWidth="1"/><col min="6" max="6" width="3" customWidth="1"/><col min="7" max="8" width="13" customWidth="1"/><col min="9" max="9" width="25" customWidth="1"/><col min="10" max="10" width="3" customWidth="1"/><col min="11" max="11" width="13" customWidth="1"/><col min="12" max="12" width="28" customWidth="1"/></cols>'
$sheet3 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"/></sheetViews>' + $cols3 + '<sheetData>' + ($s3 -join '') + '</sheetData><mergeCells count="2"><mergeCell ref="A1:L1"/><mergeCell ref="A2:L2"/></mergeCells><autoFilter ref="A3:L23"/></worksheet>')
Put (Join-Path $stageDir 'xl\worksheets\sheet3.xml') $sheet3

$s4 = New-Object System.Collections.Generic.List[string]
$s4.Add((TitleRow 1 8 'Cómo usar esta planilla' 1 30))
$s4.Add((TitleRow 2 8 'Guía rápida para preparar y cargar el ajuste de inventario inicial' 2 22))
$steps = @(
    'Completa la información general en la hoja Inventario Inicial: bodega, fecha real del conteo, motivo y observación general.',
    'Registra una fila por cada combinación de producto, marca, modelo y lote. Si un producto tiene dos lotes, usa dos filas.',
    'Para el inventario inicial deja Tipo de ajuste = Aumento. Disminución se usa para rebajar stock ya existente.',
    'Producto, marca y cantidad son obligatorios. Modelo, lote, vencimiento, series y observaciones son opcionales.',
    'Si usas números de serie, escribe uno por línea y verifica que exista uno por cada unidad de la cantidad.',
    'Revisa la hoja Resumen. Corrige filas con REVISAR o DUPLICADO antes de ingresar el ajuste.'
)
for ($i=0; $i -lt $steps.Count; $i++) { $r = 3 + $i; $s4.Add((R $r @((C "A$r" ($i+1) 3),(C "B$r" $steps[$i] 14),(C "C$r" $null 14),(C "D$r" $null 14),(C "E$r" $null 14),(C "F$r" $null 14),(C "G$r" $null 14),(C "H$r" $null 14)) 34)) }
$s4.Add((TitleRow 10 8 'Correspondencia con “Nuevo Ajuste de Inventario”' 3 24))
$map = @(
    @('Planilla','Plataforma','Regla'),
    @('Bodega','Bodega','Usa la bodega activa/principal.'),
    @('Fecha ajuste','Fecha','Fecha del inventario o ajuste.'),
    @('Motivo','Motivo','Sugerido: Inventario inicial.'),
    @('Observación general','Observación','Contexto del conteo físico.'),
    @('Producto + ID Producto','Producto','Producto obligatorio.'),
    @('Marca + ID Marca','Marca','Marca obligatoria.'),
    @('Modelo + ID Modelo','Modelo','Opcional; debe corresponder a la marca.'),
    @('Tipo de ajuste','Tipo','Para inicio: Aumento.'),
    @('Cantidad','Cantidad','Entero mayor que 0.'),
    @('Lote / Vencimiento','Lote / Fecha Vencimiento','Opcionales; lote máximo 50 caracteres.'),
    @('N° Serie(s)','N° Serie','Opcional; uno por línea, uno por unidad.'),
    @('Observación ítem','Observación del ítem','Opcional; máximo 200 caracteres.')
)
$r = 11
foreach ($item in $map) { $s4.Add((R $r @((C "A$r" $item[0] 5),(C "B$r" $item[1] 5),(C "C$r" $item[2] 14),(C "D$r" $null 14),(C "E$r" $null 14),(C "F$r" $null 14),(C "G$r" $null 14),(C "H$r" $null 14)) 28)); $r++ }
$cols4 = '<cols><col min="1" max="1" width="20" customWidth="1"/><col min="2" max="2" width="27" customWidth="1"/><col min="3" max="8" width="23" customWidth="1"/></cols>'
$merges4 = '<mergeCells count="22"><mergeCell ref="A1:H1"/><mergeCell ref="A2:H2"/>' + (($steps | ForEach-Object { '<mergeCell ref="B' + (3 + [array]::IndexOf($steps,$_)) + ':H' + (3 + [array]::IndexOf($steps,$_)) + '"/>' }) -join '') + '<mergeCell ref="A10:H10"/>' + (($map | ForEach-Object -Begin { $n=11 } -Process { $x = '<mergeCell ref="C' + $n + ':H' + $n + '"/>'; $n++; $x }) -join '') + '</mergeCells>'
$sheet4 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"/></sheetViews>' + $cols4 + '<sheetData>' + ($s4 -join '') + '</sheetData>' + $merges4 + '</worksheet>')
Put (Join-Path $stageDir 'xl\worksheets\sheet4.xml') $sheet4

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
    $required = @('[Content_Types].xml','xl/workbook.xml','xl/styles.xml','xl/worksheets/sheet1.xml','xl/worksheets/sheet2.xml','xl/worksheets/sheet3.xml','xl/worksheets/sheet4.xml')
    foreach ($name in $required) { if (-not $zip.GetEntry($name)) { throw "Falta entrada OOXML: $name" } }
} finally { $zip.Dispose() }
Write-Output ('CREATED: ' + $outputPath)
