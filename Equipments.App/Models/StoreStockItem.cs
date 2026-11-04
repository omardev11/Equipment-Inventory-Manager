namespace Equipments.App.Models;

public sealed class StoreStockItem
{
    public string StfNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public string Location { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime? LastMovementDate { get; set; }
    public string LastReceivedBy { get; set; } = string.Empty;
}
