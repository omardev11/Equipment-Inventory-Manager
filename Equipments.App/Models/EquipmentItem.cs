namespace Equipments.App.Models;

public sealed class EquipmentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int SequenceNo { get; set; }
    public string Group { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public int Quantity { get; set; } = 1;
    public string Model { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;

    public EquipmentItem Clone()
    {
        return (EquipmentItem)MemberwiseClone();
    }
}
