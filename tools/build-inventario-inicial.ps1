$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$outputDir = Join-Path $PSScriptRoot '..\outputs\01a03fc4-13fe-7a91-a529-496b6009af1e'
$outputDir = [System.IO.Path]::GetFullPath($outputDir)
$outputPath = Join-Path $outputDir 'inventario_inicial_BodegaDESAM.xlsx'
$stageDir = Join-Path 'C:\tmp' ('inventario-inicial-' + [guid]::NewGuid().ToString('N'))

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
New-Item -ItemType Directory -Path $stageDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir '_rels') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\_rels') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir 'xl\worksheets') -Force | Out-Null

function XmlEscape([string]$value) {
    if ($null -eq $value) { return '' }
    return [System.Security.SecurityElement]::Escape($value)
}

function Write-Utf8([string]$path, [string]$text) {
    [System.IO.File]::WriteAllText($path, $text, [System.Text.UTF8Encoding]::new($false))
}

function InlineCell([string]$ref, [string]$value, [int]$style = 0) {
    if ($null -eq $value) { return "<c r=`"$ref`" s=`"$style`"/>" }
    $escaped = XmlEscape $value
    return "<c r=`"$ref`" s=`"$style`" t=`"inlineStr`"><is><t>$escaped</t></is></c>"
}

function NumberCell([string]$ref, [int]$value, [int]$style = 0) {
    return "<c r=`"$ref`" s=`"$style`"><v>$value</v></c>"
}

function FormulaCell([string]$ref, [string]$formula, [int]$style = 0) {
    return "<c r=`"$ref`" s=`"$style`"><f>$(XmlEscape $formula)</f></c>"
}

function RowXml([int]$row, [string[]]$cells, [int]$height = 20) {
    return "<row r=`"$row`" ht=`"$height`" customHeight=`"1`">$($cells -join '')</row>"
}

function ColLetter([int]$number) {
    $result = ''
    while ($number -gt 0) {
        $number--
        $result = [char](65 + ($number % 26)) + $result
        $number = [math]::Floor($number / 26)
    }
    return $result
}

$cols = 1..14 | ForEach-Object { ColLetter $_ }

$styles = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <numFmts count="1"><numFmt numFmtId="164" formatCode="yyyy-mm-dd"/></numFmts>
  <fonts count="5">
    <font><sz val="10"/><color theme="1"/><name val="Aptos"/></font>
    <font><b/><sz val="16"/><color rgb="FFFFFFFF"/><name val="Aptos Display"/></font>
    <font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Aptos"/></font>
    <font><b/><sz val="10"/><color rgb="FF16324F"/><name val="Aptos"/></font>
    <font><i/><sz val="10"/><color rgb="FF52606D"/><name val="Aptos"/></font>
  </fonts>
  <fills count="9">
    <fill><patternFill patternType="none"/></fill>
    <fill><patternFill patternType="gray125"/></fill>
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
    <border><left style="medium"><color rgb="FF0F766E"/></left><right style="medium"><color rgb="FF0F766E"/></right><top style="medium"><color rgb="FF0F766E"/></top><bottom style="medium"><color rgb="FF0F766E"/></bottom><diagonal/></border>
    <border><left/><right/><top/><bottom style="thin"><color rgb="FFB8C4D1"/></bottom><diagonal/></border>
  </borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="15">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" applyAlignment="1"><alignment vertical="center"/></xf>
    <xf numFmtId="0" fontId="1" fillId="2" borderId="0" applyAlignment="1"><alignment vertical="center" horizontal="left"/></xf>
    <xf numFmtId="0" fontId="2" fillId="3" borderId="0" applyAlignment="1"><alignment vertical="center" horizontal="left"/></xf>
    <xf numFmtId="0" fontId="3" fillId="4" borderId="3" applyAlignment="1"><alignment vertical="center" horizontal="left"/></xf>
    <xf numFmtId="0" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="3" fillId="6" borderId="0" applyAlignment="1"><alignment vertical="center" horizontal="left" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="6" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="right"/></xf>
    <xf numFmtId="164" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="3" fillId="7" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="left" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="6" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="right"/></xf>
    <xf numFmtId="164" fontId="0" fillId="5" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="center"/></xf>
    <xf numFmtId="0" fontId="3" fillId="4" borderId="1" applyAlignment="1"><alignment vertical="center" horizontal="left"/></xf>
    <xf numFmtId="0" fontId="4" fillId="6" borderId="0" applyAlignment="1"><alignment vertical="top" horizontal="left" wrapText="1"/></xf>
  </cellXfs>
  <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
  <dxfs count="3">
    <dxf><fill><patternFill patternType="solid"><fgColor rgb="FFFFE0E0"/><bgColor indexed="64"/></patternFill></fill><font><b/><color rgb="FF9B1C1C"/></font></dxf>
    <dxf><fill><patternFill patternType="solid"><fgColor rgb="FFFFF4CC"/><bgColor indexed="64"/></patternFill></fill><font><b/><color rgb="FF8A5A00"/></font></dxf>
    <dxf><fill><patternFill patternType="solid"><fgColor rgb="FFE8F5E9"/><bgColor indexed="64"/></patternFill></fill><font><b/><color rgb="FF1B5E20"/></font></dxf>
  </dxfs>
</styleSheet>
'@
Write-Utf8 (Join-Path $stageDir 'xl\styles.xml') $styles

$contentTypes = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet3.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet4.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>
'@
Write-Utf8 (Join-Path $stageDir '[Content_Types].xml') $contentTypes

$rels = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
'@
Write-Utf8 (Join-Path $stageDir '_rels\.rels') $rels

$workbookRels = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml"/>
  <Relationship Id="rId4" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet4.xml"/>
  <Relationship Id="rId5" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
'@
Write-Utf8 (Join-Path $stageDir 'xl\_rels\workbook.xml.rels') $workbookRels

$workbook = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <fileVersion appName="xl" lastEdited="7" lowestEdited="7" rupBuild="1"/>
  <workbookPr defaultThemeVersion="164011"/>
  <bookViews><workbookView xWindow="120" yWindow="120" windowWidth="24000" windowHeight="12000" activeTab="0"/></bookViews>
  <sheets>
    <sheet name="Inventario Inicial" sheetId="1" r:id="rId1"/>
    <sheet name="Resumen" sheetId="2" r:id="rId2"/>
    <sheet name="Catálogos" sheetId="3" r:id="rId3"/>
    <sheet name="Instrucciones" sheetId="4" r:id="rId4"/>
  </sheets>
  <calcPr calcId="191029" calcMode="auto" fullCalcOnLoad="1" forceFullCalc="1"/>
</workbook>
'@
Write-Utf8 (Join-Path $stageDir 'xl\workbook.xml') $workbook

$sheet1Rows = New-Object System.Collections.Generic.List[string]
$sheet1Rows.Add((RowXml 1 ((1..14 | ForEach-Object { InlineCell ("$(ColLetter $_)1") ($(if ($_ -eq 1) { 'Planilla de Inventario Inicial — BodegaDESAM' } else { $null })) 1 })) 30))
$sheet1Rows.Add((RowXml 2 ((1..14 | ForEach-Object { InlineCell ("$(ColLetter $_)2") ($(if ($_ -eq 1) { 'Levantamiento físico y preparación del ajuste inicial de existencias' } else { $null })) 2 })) 22))
$sheet1Rows.Add((RowXml 3 ((1..14 | ForEach-Object { InlineCell ("$(ColLetter $_)3") $null 0 })) 8))
$sheet1Rows.Add((RowXml 4 @(
    (InlineCell 'A4' 'Bodega' 3),(InlineCell 'B4' $null 3),
    (InlineCell 'C4' 'Bodega Central DESAM' 4),(InlineCell 'D4' $null 4),(InlineCell 'E4' $null 4),(InlineCell 'F4' $null 4),
    (InlineCell 'G4' 'Fecha ajuste' 3),(InlineCell 'H4' $null 3),(InlineCell 'I4' $null 8),(InlineCell 'J4' $null 8),
    (InlineCell 'K4' 'Motivo' 3),(InlineCell 'L4' $null 3),(InlineCell 'M4' 'Inventario inicial' 4),(InlineCell 'N4' $null 4)
) 22))
$sheet1Rows.Add((RowXml 5 @(
    (InlineCell 'A5' 'Observación general' 3),(InlineCell 'B5' $null 3),(InlineCell 'C5' $null 4),(InlineCell 'D5' $null 4),(InlineCell 'E5' $null 4),(InlineCell 'F5' $null 4),(InlineCell 'G5' $null 4),(InlineCell 'H5' $null 4),(InlineCell 'I5' $null 4),(InlineCell 'J5' $null 4),(InlineCell 'K5' $null 4),(InlineCell 'L5' $null 4),(InlineCell 'M5' $null 4),(InlineCell 'N5' $null 4)
) 24))
$sheet1Rows.Add((RowXml 6 ((1..14 | ForEach-Object { InlineCell ("$(ColLetter $_)6") ($(if ($_ -eq 1) { 'Celdas amarillas = completar. Para el inventario inicial usa Tipo de ajuste = Aumento. No borres las fórmulas de Control ni los números de fila.' } else { $null })) 14 })) 26))
$sheet1Rows.Add((RowXml 7 ((1..14 | ForEach-Object { InlineCell ("$(ColLetter $_)7") $null 0 })) 8))

$headers = @('N°','ID Producto','Producto','ID Marca','Marca','ID Modelo','Modelo','Tipo de ajuste','Cantidad','Lote (opcional)','Fecha vencimiento','N° Serie(s) (uno por línea)','Observación ítem','Control')
$headerCells = for ($i=0; $i -lt $headers.Count; $i++) { InlineCell ((ColLetter ($i+1)) + '8') $headers[$i] 2 }
$sheet1Rows.Add((RowXml 8 $headerCells 42))

for ($r = 9; $r -le 108; $r++) {
    $cells = New-Object System.Collections.Generic.List[string]
    $cells.Add((NumberCell "A$r" ($r - 8) 6))
    $cells.Add((InlineCell "B$r" $null 7)); $cells.Add((InlineCell "C$r" $null 10))
    $cells.Add((InlineCell "D$r" $null 7)); $cells.Add((InlineCell "E$r" $null 10))
    $cells.Add((InlineCell "F$r" $null 7)); $cells.Add((InlineCell "G$r" $null 10))
    $cells.Add((InlineCell "H$r" 'Aumento' 4)); $cells.Add((InlineCell "I$r" $null 7))
    $cells.Add((InlineCell "J$r" $null 10)); $cells.Add((InlineCell "K$r" $null 8))
    $cells.Add((InlineCell "L$r" $null 10)); $cells.Add((InlineCell "M$r" $null 10))
    $formula = ('IF(COUNTA(B{0}:G{0},I{0}:M{0})=0,"",IF(OR(B{0}="",C{0}="",D{0}="",E{0}="",I{0}="",I{0}<1,LEN(J{0})>50,LEN(M{0})>200),"REVISAR",IF(H{0}<>"Aumento","REVISAR",IF(COUNTIFS($B$9:$B$108,B{0},$D$9:$D$108,D{0},$F$9:$F$108,F{0},$J$9:$J$108,J{0})>1,"DUPLICADO","OK"))))' -f $r)
    $cells.Add((FormulaCell "N$r" $formula 9))
    $sheet1Rows.Add((RowXml $r $cells.ToArray() 30))
}

$sheet1Cols = @'
<cols>
  <col min="1" max="1" width="6" customWidth="1"/>
  <col min="2" max="2" width="12" customWidth="1"/>
  <col min="3" max="3" width="25" customWidth="1"/>
  <col min="4" max="4" width="12" customWidth="1"/>
  <col min="5" max="5" width="20" customWidth="1"/>
  <col min="6" max="6" width="12" customWidth="1"/>
  <col min="7" max="7" width="22" customWidth="1"/>
  <col min="8" max="8" width="17" customWidth="1"/>
  <col min="9" max="9" width="11" customWidth="1"/>
  <col min="10" max="10" width="18" customWidth="1"/>
  <col min="11" max="11" width="16" customWidth="1"/>
  <col min="12" max="12" width="24" customWidth="1"/>
  <col min="13" max="13" width="23" customWidth="1"/>
  <col min="14" max="14" width="14" customWidth="1"/>
</cols>
'@
$sheet1Extras = @'
<autoFilter ref="A8:N108"/>
<conditionalFormatting sqref="N9:N108"><cfRule type="expression" dxfId="0" priority="1"><formula>N9="REVISAR"</formula></cfRule></conditionalFormatting>
<conditionalFormatting sqref="N9:N108"><cfRule type="expression" dxfId="1" priority="2"><formula>N9="DUPLICADO"</formula></cfRule></conditionalFormatting>
<conditionalFormatting sqref="N9:N108"><cfRule type="expression" dxfId="2" priority="3"><formula>N9="OK"</formula></cfRule></conditionalFormatting>
<dataValidations count="3">
  <dataValidation type="list" allowBlank="1" showInputMessage="1" showErrorMessage="1" promptTitle="Tipo de ajuste" prompt="Para inventario inicial usa Aumento." errorTitle="Valor no válido" error="Selecciona Aumento o Disminución." sqref="H9:H108"><formula1>"Aumento,Disminución"</formula1></dataValidation>
  <dataValidation type="whole" operator="greaterThan" allowBlank="1" showErrorMessage="1" errorTitle="ID no válido" error="El ID debe ser un número mayor que 0." sqref="B9:B108 D9:D108 F9:F108"><formula1>0</formula1></dataValidation>
  <dataValidation type="whole" operator="greaterThan" allowBlank="1" showErrorMessage="1" errorTitle="Cantidad no válida" error="La cantidad debe ser un entero mayor que 0." sqref="I9:I108"><formula1>0</formula1></dataValidation>
</dataValidations>
<sheetProtection selectLockedCells="0" selectUnlockedCells="0" sheet="0" objects="0" scenarios="0"/>
'@
$sheet1 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetPr><outlinePr summaryBelow="1"/></sheetPr><sheetViews><sheetView workbookViewId="0" showGridLines="0"><pane xSplit="2" ySplit="8" topLeftCell="C9" activePane="bottomRight" state="frozen"/><selection pane="bottomRight" activeCell="C9" sqref="C9"/></sheetView></sheetViews>' + $sheet1Cols + '<sheetData>' + ($sheet1Rows -join '') + '</sheetData>' + $sheet1Extras + '<mergeCells count="5"><mergeCell ref="A1:N1"/><mergeCell ref="A2:N2"/><mergeCell ref="C4:F4"/><mergeCell ref="M4:N4"/><mergeCell ref="C5:N5"/></mergeCells><pageMargins left="0.25" right="0.25" top="0.5" bottom="0.5" header="0.2" footer="0.2"/></worksheet>')
Write-Utf8 (Join-Path $stageDir 'xl\worksheets\sheet1.xml') $sheet1

$summaryRows = New-Object System.Collections.Generic.List[string]
$summaryRows.Add((RowXml 1 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)1") ($(if ($_ -eq 1) { 'Resumen de control — Inventario inicial' } else { $null })) 1 })) 30))
$summaryRows.Add((RowXml 2 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)2") ($(if ($_ -eq 1) { 'Revisa esta hoja antes de transcribir el ajuste en la plataforma.' } else { $null })) 2 })) 22))
$summaryRows.Add((RowXml 3 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)3") $null 0 })) 8))
$summaryRows.Add((RowXml 4 @((InlineCell 'A4' 'Estado general' 3),(InlineCell 'B4' $null 12),(InlineCell 'C4' $null 0),(InlineCell 'D4' $null 0),(InlineCell 'E4' 'Criterio' 3),(InlineCell 'F4' 'LISTA PARA CARGA = no hay filas con errores' 13),(InlineCell 'G4' $null 13),(InlineCell 'H4' $null 13)) 24))
$summaryRows.Add((RowXml 5 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)5") $null 0 })) 8))
$summaryRows.Add((RowXml 6 @((InlineCell 'A6' 'Total de filas utilizadas' 5),(FormulaCell 'B6' 'COUNTIF(''Inventario Inicial''!$N$9:$N$108,"OK")+COUNTIF(''Inventario Inicial''!$N$9:$N$108,"REVISAR")+COUNTIF(''Inventario Inicial''!$N$9:$N$108,"DUPLICADO")' 11),(InlineCell 'D6' 'Aumentos = unidades a ingresar' 5),(FormulaCell 'E6' 'SUMIF(''Inventario Inicial''!$H$9:$H$108,"Aumento",''Inventario Inicial''!$I$9:$I$108)' 12)) 22))
$summaryRows.Add((RowXml 7 @((InlineCell 'A7' 'Filas OK' 5),(FormulaCell 'B7' "COUNTIF('Inventario Inicial'!\$N\$9:\$N\$108,\"OK\")" 12),(InlineCell 'D7' 'Disminuciones' 5),(FormulaCell 'E7' "SUMIF('Inventario Inicial'!\$H\$9:\$H\$108,\"Disminución\",'Inventario Inicial'!\$I\$9:\$I\$108)" 12)) 22))
$summaryRows.Add((RowXml 8 @((InlineCell 'A8' 'Filas por revisar' 5),(FormulaCell 'B8' "COUNTIF('Inventario Inicial'!\$N\$9:\$N\$108,\"REVISAR\")+COUNTIF('Inventario Inicial'!\$N\$9:\$N\$108,\"DUPLICADO\")" 12),(InlineCell 'D8' 'Neto inicial' 5),(FormulaCell 'E8' 'E6-E7' 12)) 22))
$summaryRows.Add((RowXml 9 @((InlineCell 'A9' 'Nota' 3),(InlineCell 'B9' 'El inventario inicial normalmente debe tener 0 disminuciones. Si aparece alguna, revise el tipo de ajuste.' 14),(InlineCell 'C9' $null 14),(InlineCell 'D9' $null 14),(InlineCell 'E9' $null 14),(InlineCell 'F9' $null 14),(InlineCell 'G9' $null 14),(InlineCell 'H9' $null 14)) 34))
$summaryRows.Add((RowXml 10 @((InlineCell 'A10' 'Control aplicado' 3),(InlineCell 'B10' 'Producto, marca y cantidad son obligatorios; lote máximo 50 caracteres; observación máximo 200; tipo inicial = Aumento; duplicados por producto/marca/modelo/lote.' 14),(InlineCell 'C10' $null 14),(InlineCell 'D10' $null 14),(InlineCell 'E10' $null 14),(InlineCell 'F10' $null 14),(InlineCell 'G10' $null 14),(InlineCell 'H10' $null 14)) 44))
$summaryCols = '<cols><col min="1" max="1" width="27" customWidth="1"/><col min="2" max="2" width="18" customWidth="1"/><col min="3" max="3" width="3" customWidth="1"/><col min="4" max="4" width="27" customWidth="1"/><col min="5" max="5" width="18" customWidth="1"/><col min="6" max="8" width="20" customWidth="1"/></cols>'
$summary = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"/></sheetViews>' + $summaryCols + '<sheetData>' + ($summaryRows -join '') + '</sheetData><mergeCells count="5"><mergeCell ref="A1:H1"/><mergeCell ref="A2:H2"/><mergeCell ref="F4:H4"/><mergeCell ref="B9:H9"/><mergeCell ref="B10:H10"/></mergeCells></worksheet>')
Write-Utf8 (Join-Path $stageDir 'xl\worksheets\sheet2.xml') $summary

$catalogRows = New-Object System.Collections.Generic.List[string]
$catalogRows.Add((RowXml 1 ((1..12 | ForEach-Object { InlineCell ("$(ColLetter $_)1") ($(if ($_ -eq 1) { 'Catálogos de apoyo' } else { $null })) 1 })) 30))
$catalogRows.Add((RowXml 2 ((1..12 | ForEach-Object { InlineCell ("$(ColLetter $_)2") ($(if ($_ -eq 1) { 'Completa o copia aquí los nombres e IDs que ves en la plataforma para evitar errores de transcripción.' } else { $null })) 2 })) 22))
$catalogRows.Add((RowXml 3 @((InlineCell 'A3' 'ID Producto' 2),(InlineCell 'B3' 'Producto' 2),(InlineCell 'C3' $null 2),(InlineCell 'D3' 'ID Marca' 2),(InlineCell 'E3' 'Marca' 2),(InlineCell 'F3' $null 2),(InlineCell 'G3' 'ID Modelo' 2),(InlineCell 'H3' 'ID Marca' 2),(InlineCell 'I3' 'Modelo' 2),(InlineCell 'J3' $null 2),(InlineCell 'K3' 'ID Bodega' 2),(InlineCell 'L3' 'Bodega' 2)) 32))
$brandRows = @('Genérica','3M','Kimberly-Clark')
for ($r=4; $r -le 23; $r++) {
    $brand = if ($r -le 6) { $brandRows[$r-4] } else { $null }
    $catalogRows.Add((RowXml $r @((InlineCell "A$r" $null 7),(InlineCell "B$r" $null 10),(InlineCell "C$r" $null 0),(InlineCell "D$r" $null 7),(InlineCell "E$r" $brand 10),(InlineCell "F$r" $null 0),(InlineCell "G$r" $null 7),(InlineCell "H$r" $null 7),(InlineCell "I$r" $null 10),(InlineCell "J$r" $null 0),(InlineCell "K$r" $(if ($r -eq 4) { $null } else { $null }) 7),(InlineCell "L$r" $(if ($r -eq 4) { 'Bodega Central DESAM' } else { $null }) 10)) 22))
}
$catalogCols = '<cols><col min="1" max="1" width="13" customWidth="1"/><col min="2" max="2" width="28" customWidth="1"/><col min="3" max="3" width="3" customWidth="1"/><col min="4" max="4" width="13" customWidth="1"/><col min="5" max="5" width="22" customWidth="1"/><col min="6" max="6" width="3" customWidth="1"/><col min="7" max="8" width="13" customWidth="1"/><col min="9" max="9" width="25" customWidth="1"/><col min="10" max="10" width="3" customWidth="1"/><col min="11" max="11" width="13" customWidth="1"/><col min="12" max="12" width="28" customWidth="1"/></cols>'
$catalog = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"/></sheetViews>' + $catalogCols + '<sheetData>' + ($catalogRows -join '') + '</sheetData><mergeCells count="2"><mergeCell ref="A1:L1"/><mergeCell ref="A2:L2"/></mergeCells><autoFilter ref="A3:L23"/></worksheet>')
Write-Utf8 (Join-Path $stageDir 'xl\worksheets\sheet3.xml') $catalog

$instructionRows = New-Object System.Collections.Generic.List[string]
$instructionRows.Add((RowXml 1 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)1") ($(if ($_ -eq 1) { 'Cómo usar esta planilla' } else { $null })) 1 })) 30))
$instructionRows.Add((RowXml 2 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)2") ($(if ($_ -eq 1) { 'Guía rápida para preparar y cargar el ajuste de inventario inicial' } else { $null })) 2 })) 22))
$instructionRows.Add((RowXml 3 @((InlineCell 'A3' '1' 3),(InlineCell 'B3' 'Completa la información general en la hoja Inventario Inicial: bodega, fecha real del conteo, motivo y observación general.' 14),(InlineCell 'C3' $null 14),(InlineCell 'D3' $null 14),(InlineCell 'E3' $null 14),(InlineCell 'F3' $null 14),(InlineCell 'G3' $null 14),(InlineCell 'H3' $null 14)) 34))
$instructionRows.Add((RowXml 4 @((InlineCell 'A4' '2' 3),(InlineCell 'B4' 'Registra una fila por cada combinación de producto, marca, modelo y lote. Si un producto tiene dos lotes, usa dos filas.' 14),(InlineCell 'C4' $null 14),(InlineCell 'D4' $null 14),(InlineCell 'E4' $null 14),(InlineCell 'F4' $null 14),(InlineCell 'G4' $null 14),(InlineCell 'H4' $null 14)) 34))
$instructionRows.Add((RowXml 5 @((InlineCell 'A5' '3' 3),(InlineCell 'B5' 'Para el inventario inicial deja Tipo de ajuste = Aumento. El sistema utiliza Disminución solo para rebajar stock ya existente.' 14),(InlineCell 'C5' $null 14),(InlineCell 'D5' $null 14),(InlineCell 'E5' $null 14),(InlineCell 'F5' $null 14),(InlineCell 'G5' $null 14),(InlineCell 'H5' $null 14)) 34))
$instructionRows.Add((RowXml 6 @((InlineCell 'A6' '4' 3),(InlineCell 'B6' 'Producto, marca y cantidad son obligatorios. Modelo, lote, fecha de vencimiento, series y observaciones son opcionales.' 14),(InlineCell 'C6' $null 14),(InlineCell 'D6' $null 14),(InlineCell 'E6' $null 14),(InlineCell 'F6' $null 14),(InlineCell 'G6' $null 14),(InlineCell 'H6' $null 14)) 34))
$instructionRows.Add((RowXml 7 @((InlineCell 'A7' '5' 3),(InlineCell 'B7' 'Si usas números de serie, escribe uno por línea y verifica que exista uno por cada unidad de la cantidad. La aplicación valida esa correspondencia.' 14),(InlineCell 'C7' $null 14),(InlineCell 'D7' $null 14),(InlineCell 'E7' $null 14),(InlineCell 'F7' $null 14),(InlineCell 'G7' $null 14),(InlineCell 'H7' $null 14)) 34))
$instructionRows.Add((RowXml 8 @((InlineCell 'A8' '6' 3),(InlineCell 'B8' 'Revisa la hoja Resumen. Corrige las filas con REVISAR o DUPLICADO antes de ingresar el ajuste.' 14),(InlineCell 'C8' $null 14),(InlineCell 'D8' $null 14),(InlineCell 'E8' $null 14),(InlineCell 'F8' $null 14),(InlineCell 'G8' $null 14),(InlineCell 'H8' $null 14)) 34))
$instructionRows.Add((RowXml 10 ((1..8 | ForEach-Object { InlineCell ("$(ColLetter $_)10") ($(if ($_ -eq 1) { 'Correspondencia con “Nuevo Ajuste de Inventario”' } else { $null })) 3 })) 24))
$mapping = @(
    @('Planilla','Plataforma','Regla'),
    @('Bodega','Bodega','Usa la bodega activa/principal.'),
    @('Fecha ajuste','Fecha','Fecha del inventario o del ajuste.'),
    @('Motivo','Motivo','Sugerido: Inventario inicial.'),
    @('Observación general','Observación','Contexto del conteo físico.'),
    @('Producto + ID Producto','Producto','Producto obligatorio; confirma el ID en la plataforma.'),
    @('Marca + ID Marca','Marca','Marca obligatoria.'),
    @('Modelo + ID Modelo','Modelo','Opcional; debe corresponder a la marca.'),
    @('Tipo de ajuste','Tipo','Para inicio: Aumento.'),
    @('Cantidad','Cantidad','Entero mayor que 0.'),
    @('Lote / Fecha vencimiento','Lote / Fecha Vencimiento','Opcionales; lote máximo 50 caracteres.'),
    @('N° Serie(s)','N° Serie','Opcional; uno por línea, uno por unidad.'),
    @('Observación ítem','Observación del ítem','Opcional; máximo 200 caracteres.')
)
$mapRow = 11
foreach ($item in $mapping) {
    $instructionRows.Add((RowXml $mapRow @((InlineCell "A$mapRow" $item[0] 5),(InlineCell "B$mapRow" $item[1] 5),(InlineCell "C$mapRow" $item[2] 14),(InlineCell "D$mapRow" $null 14),(InlineCell "E$mapRow" $null 14),(InlineCell "F$mapRow" $null 14),(InlineCell "G$mapRow" $null 14),(InlineCell "H$mapRow" $null 14)) 28))
    $mapRow++
}
$instructionCols = '<cols><col min="1" max="1" width="18" customWidth="1"/><col min="2" max="2" width="25" customWidth="1"/><col min="3" max="8" width="22" customWidth="1"/></cols>'
$mergeXml = for ($m=11; $m -lt $mapRow; $m++) { '<mergeCell ref="C' + $m + ':H' + $m + '"/>' }
$instruction = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0" showGridLines="0"/></sheetViews>' + $instructionCols + '<sheetData>' + ($instructionRows -join '') + '</sheetData><mergeCells count="22"><mergeCell ref="A1:H1"/><mergeCell ref="A2:H2"/><mergeCell ref="B3:H3"/><mergeCell ref="B4:H4"/><mergeCell ref="B5:H5"/><mergeCell ref="B6:H6"/><mergeCell ref="B7:H7"/><mergeCell ref="B8:H8"/><mergeCell ref="A10:H10"/>' + ($mergeXml -join '') + '</mergeCells></worksheet>')
Write-Utf8 (Join-Path $stageDir 'xl\worksheets\sheet4.xml') $instruction

if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory($stageDir, $outputPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)

$zip = [System.IO.Compression.ZipFile]::OpenRead($outputPath)
try {
    $required = @('[Content_Types].xml','xl/workbook.xml','xl/styles.xml','xl/worksheets/sheet1.xml','xl/worksheets/sheet2.xml','xl/worksheets/sheet3.xml','xl/worksheets/sheet4.xml')
    foreach ($entryName in $required) {
        if (-not $zip.GetEntry($entryName)) { throw "Falta la entrada OOXML requerida: $entryName" }
    }
} finally { $zip.Dispose() }

Write-Output "CREATED: $outputPath"
