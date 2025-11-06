namespace TechHaven.Shared.DTOs.Payments;

public class PaymentDto
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }
}


