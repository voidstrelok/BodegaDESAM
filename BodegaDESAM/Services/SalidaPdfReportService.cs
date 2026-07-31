using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BodegaDESAM.Services
{
    public class SalidaPdfReportService
    {
        // Background / border custom colours (hex strings work for IContainer)
        private const string CPrimary   = "#1B3A5C";
        private const string CSecondary = "#2E6DA4";
        private const string CAccent    = "#3A8FD1";
        private const string CRowAlt    = "#EDF4FB";
        private const string CBorder    = "#C8DAEA";
        private const string CTotalBg   = "#D9E8F5";
        private const string CRowWhite  = "#FFFFFF";

        public byte[] CreatePdf(Salida salida, string? nombreQuienEntrega)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(36);
                    page.MarginVertical(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("DejaVu Sans").FontColor(Colors.Black));

                    // ── HEADER ────────────────────────────────────────────
                    page.Header().Column(col =>
                    {
                        // Banner
                        col.Item()
                           .Background(CPrimary)
                           .PaddingHorizontal(16).PaddingVertical(12)
                           .Row(row =>
                           {
                               row.RelativeItem().Column(c =>
                               {
                                   c.Item().Text("BODEGA CENTRAL — DESAM")
                                          .FontSize(15).Bold().FontColor(Colors.White);
                                   c.Item().Text("Despacho de Insumos")
                                          .FontSize(10).FontColor(Colors.LightBlue.Lighten3);
                               });
                               row.ConstantItem(135).AlignMiddle().AlignRight().Column(c =>
                               {
                                   c.Item().Background(CSecondary)
                                          .PaddingHorizontal(10).PaddingVertical(7)
                                          .Text($"SALIDA  N° {salida.Id:D6}")
                                          .FontSize(12).Bold().FontColor(Colors.White);
                               });
                           });

                        // Accent stripe
                        col.Item().Height(4).Background(CAccent);

                        // Metadata grid
                        col.Item().PaddingTop(10).Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                            });

                            void MetaCell(string label, string value) =>
                                t.Cell().Border(1).BorderColor(CBorder).Padding(7).Column(c =>
                                {
                                    c.Item().Text(label).FontSize(7.5f).Bold().FontColor(Colors.BlueGrey.Darken1);
                                    c.Item().Text(value).FontSize(10).Bold();
                                });

                            MetaCell("FECHA",           salida.Fecha.ToString("dd/MM/yyyy"));
                            MetaCell("BODEGA",          salida.Bodega?.Nombre ?? "—");
                            MetaCell("ESTABLECIMIENTO", salida.EStablecimiento?.Nombre ?? "—");
                            MetaCell("SOLICITANTE",     salida.Solicitante ?? "—");
                            t.Cell().ColumnSpan(2).Border(1).BorderColor(CBorder).Padding(7).Column(c =>
                            {
                                c.Item().Text("OBSERVACIONES").FontSize(7.5f).Bold().FontColor(Colors.BlueGrey.Darken1);
                                c.Item().Text(!string.IsNullOrWhiteSpace(salida.Observacion)
                                    ? salida.Observacion : "—").FontSize(10);
                            });
                        });

                        col.Item().Height(12);
                    });

                    // ── CONTENT ───────────────────────────────────────────
                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        // Section heading
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(4).Background(CAccent);
                            r.RelativeItem().PaddingLeft(8).PaddingVertical(5)
                                           .Text("DETALLE DE DESPACHO")
                                           .FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                        });

                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3.8f); // Producto y series
                                columns.RelativeColumn(2.1f); // Marca y modelo
                                columns.RelativeColumn(2.8f); // Lote y origen
                                columns.RelativeColumn(1.0f); // Cantidad
                            });

                            table.Header(header =>
                            {
                                IContainer HCell(IContainer c) =>
                                    c.Background(CPrimary)
                                     .PaddingVertical(7).PaddingHorizontal(7);

                                header.Cell().Element(HCell)
                                      .Text("PRODUCTO").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell)
                                      .Text("MARCA Y MODELO").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell)
                                      .Text("LOTE Y ORIGEN").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell).AlignRight()
                                      .Text("CANT.").FontSize(8.5f).Bold().FontColor(Colors.White);
                            });

                            int rowIdx = 0;
                            long totalCantidad = 0;

                            foreach (var d in salida.DetalleSalida)
                            {
                                bool isAlt = rowIdx++ % 2 == 1;
                                string rowBg = isAlt ? CRowAlt : CRowWhite;

                                IContainer BCell(IContainer c) =>
                                    c.Background(rowBg)
                                     .PaddingVertical(7).PaddingHorizontal(7)
                                     .BorderBottom(1).BorderColor(CBorder);

                                var seriesDet = salida.ProductoSeries
                                    .Where(s => s.id_producto == d.id_producto)
                                    .Select(s => s.Serie)
                                    .ToList();

                                // Proveedor y N° Factura desde la Entrada original del Lote
                                var entradaOrigen = d.DetalleEntrada?.Entrada;
                                var proveedorNombre = entradaOrigen?.Proveedor?.Nombre ?? "—";
                                var nFactura        = entradaOrigen?.NDocumento ?? "—";
                                var fechaVenc       = d.DetalleEntrada?.FechaVencimiento;

                                table.Cell().Element(BCell).Column(c =>
                                {
                                    c.Item().Text(d.Producto?.CategoriaProducto?.Nombre ?? "—").FontSize(7.5f).FontColor(Colors.BlueGrey.Darken1);                                    
                                    c.Item().Text(d.Producto?.Nombre ?? "—");
                                    if (seriesDet.Count > 0)
                                        c.Item().Text("S/N: " + string.Join(", ", seriesDet))
                                               .FontSize(7.5f).FontColor(Colors.BlueGrey.Darken1);

                                });
                                table.Cell().Element(BCell).Column(c =>
                                {
                                    c.Item().Text(d.Marca?.Nombre ?? "—");
                                    c.Item().PaddingTop(2).Text(d.Modelo?.Nombre ?? "—")
                                     .FontSize(8.5f).FontColor(Colors.BlueGrey.Darken1);
                                });
                                table.Cell().Element(BCell).Column(c =>
                                {
                                    c.Item().Text($"Lote: {d.Lote?.Codigo ?? "—"}");
                                    c.Item().Text($"Vence: {(fechaVenc.HasValue ? fechaVenc.Value.ToString("dd/MM/yyyy") : "—")}")
                                     .FontSize(8.5f);
                                    c.Item().PaddingTop(2).Text(proveedorNombre)
                                     .FontSize(8.5f).FontColor(Colors.BlueGrey.Darken1);
                                    c.Item().Text($"Factura: {nFactura}")
                                     .FontSize(8.5f).FontColor(Colors.BlueGrey.Darken1);
                                });
                                table.Cell().Element(BCell).AlignRight()
                                     .Text(d.Cantidad.ToString());

                                totalCantidad += d.Cantidad;
                            }

                            // Total row
                            IContainer TCell(IContainer c) =>
                                c.Background(CTotalBg)
                                 .PaddingVertical(6).PaddingHorizontal(7)
                                 .BorderTop(1).BorderColor(CSecondary);

                            table.Cell().ColumnSpan(3).Element(TCell).AlignRight()
                                 .Text("TOTAL UNIDADES").FontSize(8.5f).Bold().FontColor(Colors.Blue.Darken4);
                            table.Cell().Element(TCell).AlignRight()
                                 .Text(totalCantidad.ToString()).Bold().FontColor(Colors.Blue.Darken4);
                        });

                        // Signatures
                        col.Item().PaddingTop(32).Row(row =>
                        {
                            void SigBox(RowDescriptor r, string role, string? nombre)
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(role).FontSize(8.5f).Bold().FontColor(Colors.BlueGrey.Darken1);
                                    c.Item().PaddingTop(36).BorderBottom(1).BorderColor(CPrimary);
                                    c.Item().PaddingTop(4)
                                            .Text(string.IsNullOrWhiteSpace(nombre) ? "—" : nombre)
                                            .FontSize(9).Bold();
                                    c.Item().PaddingTop(2)
                                            .Text("Firma / Fecha")
                                            .FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                                });
                            }

                            SigBox(row, "SOLICITA", salida.Solicitante);
                            row.ConstantItem(50);
                            SigBox(row, "ENTREGA", nombreQuienEntrega);
                        });
                    });

                    // ── FOOTER ────────────────────────────────────────────
                    page.Footer().Column(col =>
                    {
                        col.Item().BorderTop(1).BorderColor(CBorder).PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem()
                             .Text("Bodega Central — Sistema DESAM")
                             .FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                            r.RelativeItem().AlignRight().Text(x =>
                            {
                                x.Span("Página ").FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                                x.CurrentPageNumber().FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                                x.Span(" de ").FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                                x.TotalPages().FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                                x.Span($"  —  Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                 .FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                            });
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
