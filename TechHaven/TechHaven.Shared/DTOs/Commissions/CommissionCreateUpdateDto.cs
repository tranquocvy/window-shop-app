namespace TechHaven.Shared.DTOs.Commissions;

public class CommissionCreateUpdateDto
{
    public int UserId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal TotalSales { get; set; }

    public decimal CommissionRate { get; set; }

    public string? Note { get; set; }
}