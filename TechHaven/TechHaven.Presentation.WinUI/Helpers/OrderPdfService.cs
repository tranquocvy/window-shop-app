using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Orders;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF.Drawing;
using System.Reflection;
using System.Net.Http;

namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// PDF generator using QuestPDF.
    /// Uses a fixed remote logo URL for the order header.
    /// </summary>
    public class OrderPdfService : IOrderPdfService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Remote logo URL (always used)
        private const string RemoteLogoUrl = "https://qweidvjhsgecpcdskvop.supabase.co/storage/v1/object/public/techhaven-images/72e4a4ca-5fc4-4f0a-81fc-776b3b93a02a.png";

        public OrderPdfService()
        {
            // No local asset lookup - logo will be downloaded on demand from RemoteLogoUrl
        }

        public async Task<byte[]> GenerateOrderPdfAsync(OrderDto order)
        {
            if (order == null) throw new System.ArgumentNullException(nameof(order));

            byte[]? logoBytes = null;

            try
            {
                using var resp = await _httpClient.GetAsync(RemoteLogoUrl);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    if (bytes != null && bytes.Length > 0)
                        logoBytes = bytes;
                }
            }
            catch
            {
                logoBytes = null;
            }

            using var ms = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header with logo from remote URL
                    page.Header().Row(row =>
                    {
                        if (logoBytes != null)
                        {
                            row.ConstantColumn(100).Height(60).AlignMiddle().AlignLeft().Element(c =>
                            {
                                c.Image(logoBytes, ImageScaling.FitArea);
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
                                    table.Cell().Element(CellBody).AlignRight().Text($"{d.SubTotal:N0} VND");
                                }

                                table.Cell().ColumnSpan(3).Height(6);

                                table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Subtotal:").SemiBold();
                                table.Cell().Element(CellBody).AlignRight().Text($"{order.SubtotalAmount:N0} VND");

                                table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Discount:").SemiBold();
                                table.Cell().Element(CellBody).AlignRight().Text($"{order.Discount:N0} VND");

                                table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Total:").SemiBold().FontSize(12);
                                table.Cell().Element(CellBody).AlignRight().Text($"{order.TotalAmount:N0} VND").FontSize(12);

                                static IContainer CellHeader(IContainer container) => container.Padding(8).Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                static IContainer CellBody(IContainer container) => container.Padding(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            });
                        });

                        col.Item().Height(10);

                        col.Item().AlignCenter().Text("Thank you for shopping at TechHaven!").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    page.Footer().AlignCenter().Element(f => f.Text($"TechHaven © {System.DateTime.Now.Year}").FontSize(9).FontColor(Colors.Grey.Darken2).SemiBold());
                });
            }).GeneratePdf(ms);

            return ms.ToArray();
        }
    }
}
