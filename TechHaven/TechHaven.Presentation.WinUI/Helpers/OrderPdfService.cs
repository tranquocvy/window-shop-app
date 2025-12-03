using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Orders;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF.Drawing;
using System.Linq;
using System.Reflection;

namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// Production-ready PDF generator using QuestPDF.
    /// Attempts to include an optional logo from the application's assets (prefers StoreLogo.png).
    /// </summary>
    public class OrderPdfService : IOrderPdfService
    {
        private readonly byte[]? _logoBytes;

        public OrderPdfService()
        {
            // Try load logo from application directory (assets/StoreLogo.png, assets/logo.png, or StoreLogo.png)
            try
            {
                var baseDir = System.AppContext.BaseDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                var candidates = new[]
                {
                    Path.Combine(baseDir, "assets", "StoreLogo.png"),
                    Path.Combine(baseDir, "assets", "logo.png"),
                    Path.Combine(baseDir, "StoreLogo.png"),
                };

                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        _logoBytes = File.ReadAllBytes(candidate);
                        break;
                    }
                }
            }
            catch
            {
                _logoBytes = null;
            }
        }

        public Task<byte[]> GenerateOrderPdfAsync(OrderDto order)
        {
            if (order == null) throw new System.ArgumentNullException(nameof(order));

            using var ms = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header with optional logo
                    page.Header().Row(row =>
                    {
                        if (_logoBytes != null)
                        {
                            row.ConstantColumn(100).Height(60).AlignMiddle().AlignLeft().Element(c =>
                            {
                                c.Image(_logoBytes, ImageScaling.FitArea);
                            });
                        }
                        else
                        {
                            row.ConstantColumn(100).Height(60).AlignMiddle().AlignLeft().Element(c => { c.Text("\n"); });
                        }

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text("TechHaven").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Order Receipt").FontSize(11).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantColumn(220).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Date: {order.OrderDate:dd/MM/yyyy HH:mm}").FontSize(11);
                            col.Item().Text($"Customer: {order.CustomerName ?? "(Walk-in)"}").FontSize(11);
                        });
                    });

                    page.Content().PaddingTop(8).Column(col =>
                    {
                        // Items table
                        col.Item().Element(c =>
                        {
                            c.Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(6);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellHeader).Text("Product");
                                    header.Cell().Element(CellHeader).AlignCenter().Text("Qty");
                                    header.Cell().Element(CellHeader).AlignRight().Text("Subtotal");
                                });

                                foreach (var d in order.Details ?? System.Array.Empty<OrderDetailDto>())
                                {
                                    table.Cell().Element(CellBody).Text(d.ProductName ?? string.Empty);
                                    table.Cell().Element(CellBody).AlignCenter().Text(d.Quantity.ToString());
                                    table.Cell().Element(CellBody).AlignRight().Text($"{d.SubTotal:N0} ?");
                                }

                                table.Cell().ColumnSpan(3).Height(6);

                                table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Subtotal:").SemiBold();
                                table.Cell().Element(CellBody).AlignRight().Text($"{order.SubtotalAmount:N0} ?");

                                table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Discount:").SemiBold();
                                table.Cell().Element(CellBody).AlignRight().Text($"{order.Discount:N0} ?");

                                table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Total:").SemiBold().FontSize(12);
                                table.Cell().Element(CellBody).AlignRight().Text($"{order.TotalAmount:N0} ?").FontSize(12);

                                static IContainer CellHeader(IContainer container) => container.Padding(8).Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                static IContainer CellBody(IContainer container) => container.Padding(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            });
                        });

                        // Notes
                        col.Item().PaddingTop(8).Element(c =>
                        {
                            c.Border(1).BorderColor(Colors.Grey.Lighten3).Padding(10).Column(nc =>
                            {
                                nc.Item().Text("Notes").SemiBold();
                                nc.Item().Text(order.Notes ?? "(none)").FontSize(10).FontColor(Colors.Grey.Darken1);
                            });
                        });

                        col.Item().Height(10);

                        col.Item().AlignCenter().Text("Thank you for shopping at TechHaven!").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    page.Footer().AlignCenter().Element(f => f.Text($"TechHaven © {System.DateTime.Now.Year}").FontSize(9).FontColor(Colors.Grey.Darken2).SemiBold());
                });
            }).GeneratePdf(ms);

            return Task.FromResult(ms.ToArray());
        }
    }
}
