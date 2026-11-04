namespace Equipments.App.Models;

public sealed class StfRequestItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RowNo { get; set; }
    public string StfNo { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.Today;
    public string RequestedBy { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public decimal RequestedQuantity { get; set; } = 1;
    public decimal ReceivedQuantity { get; set; }
    public string Status { get; set; } = "Bekliyor";
    public string Description { get; set; } = string.Empty;

    public decimal RemainingQuantity => Math.Max(0, RequestedQuantity - ReceivedQuantity);

    public StfRequestItem Clone()
    {
        return (StfRequestItem)MemberwiseClone();
    }
}
