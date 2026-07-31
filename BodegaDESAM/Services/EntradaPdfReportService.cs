using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BodegaDESAM.Services
{
    public class EntradaPdfReportService
    {
        // Background / border custom colours (hex strings work for IContainer)
        private const string CPrimary   = "#1B3A5C";
        private const string CSecondary = "#2E6DA4";
        private const string CAccent    = "#3A8FD1";
        private const string CRowAlt    = "#EDF4FB";
        private const string CBorder    = "#C8DAEA";
        private const string CTotalBg   = "#D9E8F5";
        private const string CRowWhite  = "#FFFFFF";

        public byte[] CreatePdf(Entrada entrada)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(36);
                    page.MarginVertical(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

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
                                   c.Item().Text("Recepción de Insumos")
                                          .FontSize(10).FontColor(Colors.LightBlue.Lighten3);
                               });
                               row.ConstantItem(160).AlignMiddle().AlignRight().Column(c =>
                               {
                                   c.Item().Background(CSecondary)
                                          .PaddingHorizontal(10).PaddingVertical(7)
                                          .Text($"ENTRADA  N° {entrada.Id:D6}")
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
                                cd.RelativeColumn();
                            });

                            void MetaCell(string label, string value) =>
                                t.Cell().Border(1).BorderColor(CBorder).Padding(7).Column(c =>
                                {
                                    c.Item().Text(label).FontSize(7.5f).Bold().FontColor(Colors.BlueGrey.Darken1);
                                    c.Item().Text(value).FontSize(10).Bold();
                                });

                            MetaCell("FECHA",        entrada.Fecha.ToString("dd/MM/yyyy"));
                            MetaCell("BODEGA",       entrada.Bodega?.Nombre ?? "—");
                            MetaCell("N° DOCUMENTO", entrada.NDocumento);
                            MetaCell("PROVEEDOR",    entrada.Proveedor?.Nombre ?? "—");
                            t.Cell().ColumnSpan(2).Border(1).BorderColor(CBorder).Padding(7).Column(c =>
                            {
                                c.Item().Text("OBSERVACIONES").FontSize(7.5f).Bold().FontColor(Colors.BlueGrey.Darken1);
                                c.Item().Text(!string.IsNullOrWhiteSpace(entrada.Observacion)
                                    ? entrada.Observacion : "—").FontSize(10);
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
                                           .Text("DETALLE DE RECEPCIÓN")
                                           .FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                        });

                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4.5f); // Producto
                                columns.RelativeColumn(2);    // Marca
                                columns.RelativeColumn(2);    // Modelo
                                columns.RelativeColumn(2);    // Lote
                                columns.RelativeColumn(2);    // F. Venc.
                                columns.RelativeColumn(1.2f); // Cant.
                            });

                            table.Header(header =>
                            {
                                IContainer HCell(IContainer c) =>
                                    c.Background(CPrimary)
                                     .PaddingVertical(7).PaddingHorizontal(7);

                                header.Cell().Element(HCell)
                                      .Text("PRODUCTO").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell)
                                      .Text("MARCA").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell)
                                      .Text("MODELO").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell)
                                      .Text("LOTE").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell)
                                      .Text("F. VENC.").FontSize(8.5f).Bold().FontColor(Colors.White);
                                header.Cell().Element(HCell).AlignRight()
                                      .Text("CANT.").FontSize(8.5f).Bold().FontColor(Colors.White);
                            });

                            int rowIdx = 0;
                            long totalCantidad = 0;

                            foreach (var d in entrada.DetalleEntrada)
                            {
                                bool isAlt = rowIdx++ % 2 == 1;
                                string rowBg = isAlt ? CRowAlt : CRowWhite;

                                IContainer BCell(IContainer c) =>
                                    c.Background(rowBg)
                                     .PaddingVertical(6).PaddingHorizontal(7)
                                     .BorderBottom(1).BorderColor(CBorder);

                                table.Cell().Element(BCell).Column(c =>
                                {
                                    c.Item().Text(d.Producto?.Nombre ?? "—");
                                    if (d.ProductoSeries.Count > 0)
                                        c.Item().Text("S/N: " + string.Join(", ", d.ProductoSeries.Select(s => s.Serie)))
                                               .FontSize(7.5f).FontColor(Colors.BlueGrey.Darken1);
                                });
                                table.Cell().Element(BCell).Text(d.Marca?.Nombre ?? "—");
                                table.Cell().Element(BCell).Text(d.Modelo?.Nombre ?? "—");
                                table.Cell().Element(BCell).Text(d.Lote?.Codigo ?? "—");
                                table.Cell().Element(BCell).Text(d.FechaVencimiento.HasValue
                                    ? d.FechaVencimiento.Value.ToString("dd/MM/yyyy") : "—");
                                table.Cell().Element(BCell).AlignRight()
                                     .Text(d.Cantidad.ToString());

                                totalCantidad += d.Cantidad;
                            }

                            // Total row
                            IContainer TCell(IContainer c) =>
                                c.Background(CTotalBg)
                                 .PaddingVertical(6).PaddingHorizontal(7)
                                 .BorderTop(1).BorderColor(CSecondary);

                            table.Cell().ColumnSpan(5).Element(TCell).AlignRight()
                                 .Text("TOTAL UNIDADES").FontSize(8.5f).Bold().FontColor(Colors.Blue.Darken4);
                            table.Cell().Element(TCell).AlignRight()
                                 .Text(totalCantidad.ToString()).Bold().FontColor(Colors.Blue.Darken4);
                        });

                        // Signatures
                        col.Item().PaddingTop(32).Row(row =>
                        {
                            void SigBox(RowDescriptor r, string role)
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(role).FontSize(8.5f).Bold().FontColor(Colors.BlueGrey.Darken1);
                                    c.Item().PaddingTop(36).BorderBottom(1).BorderColor(CPrimary);
                                    c.Item().PaddingTop(4)
                                            .Text("Firma / Nombre / Fecha")
                                            .FontSize(8).FontColor(Colors.BlueGrey.Darken1);
                                });
                            }

                            SigBox(row, "RECIBE");
                            row.ConstantItem(50);
                            SigBox(row, "PROVEEDOR");
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
