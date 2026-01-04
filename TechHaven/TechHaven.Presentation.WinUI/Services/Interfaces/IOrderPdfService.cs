using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IOrderPdfService
    {
        Task<byte[]> GenerateOrderPdfAsync(OrderDto order);
    }
}
