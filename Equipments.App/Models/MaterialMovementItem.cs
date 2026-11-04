namespace Equipments.App.Models;

public sealed class MaterialMovementItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? RelatedRequestId { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.Today;
    public string MovementType { get; set; } = "Teslim";
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public decimal Quantity { get; set; } = 1;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public string DeliveredBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public MaterialMovementItem Clone()
    {
        return (MaterialMovementItem)MemberwiseClone();
    }
}
