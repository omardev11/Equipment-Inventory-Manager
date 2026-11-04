using System.Text.Json;
using Equipments.App.Models;

namespace Equipments.App.Services;

public sealed class MaterialMovementRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public MaterialMovementRepository(string filePath)
    {
        _filePath = filePath;
    }

    public List<MaterialMovementItem> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        var items = JsonSerializer.Deserialize<List<MaterialMovementItem>>(json, JsonOptions) ?? [];
        return Normalize(items);
    }

    public void Save(IEnumerable<MaterialMovementItem> items)
    {
        var normalized = Normalize(items)
            .OrderByDescending(x => x.MovementDate)
            .ThenBy(x => x.ProductName)
            .ToList();
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllText(_filePath, json);
    }

    public static List<MaterialMovementItem> Normalize(IEnumerable<MaterialMovementItem> items)
    {
        return items
            .Where(x => x is not null)
            .Select(x =>
            {
                x.Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id;
                x.MovementType = string.IsNullOrWhiteSpace(x.MovementType) ? "Teslim" : x.MovementType.Trim();
                x.ProductName = (x.ProductName ?? string.Empty).Trim();
                x.Unit = string.IsNullOrWhiteSpace(x.Unit) ? "Adet" : x.Unit.Trim();
                x.FromLocation = (x.FromLocation ?? string.Empty).Trim();
                x.ToLocation = (x.ToLocation ?? string.Empty).Trim();
                x.ReceivedBy = (x.ReceivedBy ?? string.Empty).Trim();
                x.DeliveredBy = (x.DeliveredBy ?? string.Empty).Trim();
                x.Notes = (x.Notes ?? string.Empty).Trim();
                x.Quantity = x.Quantity <= 0 ? 1 : x.Quantity;
                return x;
            })
            .ToList();
    }
}
