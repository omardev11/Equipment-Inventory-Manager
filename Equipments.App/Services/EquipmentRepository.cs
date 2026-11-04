using System.Text.Json;
using Equipments.App.Models;

namespace Equipments.App.Services;

public sealed class EquipmentRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public EquipmentRepository(string filePath)
    {
        _filePath = filePath;
    }

    public List<EquipmentItem> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        var items = JsonSerializer.Deserialize<List<EquipmentItem>>(json, JsonOptions) ?? [];
        return Normalize(items);
    }

    public void Save(IEnumerable<EquipmentItem> items)
    {
        var normalized = Normalize(items).OrderBy(x => x.SequenceNo).ToList();
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllText(_filePath, json);
    }

    public List<EquipmentItem> ReplaceAll(IEnumerable<EquipmentItem> items)
    {
        var normalized = Normalize(items);
        Resequence(normalized);
        Save(normalized);
        return normalized;
    }

    public static List<EquipmentItem> Normalize(IEnumerable<EquipmentItem> items)
    {
        return items
            .Where(x => x is not null)
            .Select(x =>
            {
                x.Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id;
                x.Group = (x.Group ?? string.Empty).Trim();
                x.Name = (x.Name ?? string.Empty).Trim();
                x.Unit = string.IsNullOrWhiteSpace(x.Unit) ? "Adet" : x.Unit.Trim();
                x.Model = (x.Model ?? string.Empty).Trim();
                x.SerialNo = (x.SerialNo ?? string.Empty).Trim();
                x.Location = (x.Location ?? string.Empty).Trim();
                x.Owner = (x.Owner ?? string.Empty).Trim();
                x.Quantity = x.Quantity <= 0 ? 1 : x.Quantity;
                return x;
            })
            .ToList();
    }

    public static void Resequence(IList<EquipmentItem> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            items[index].SequenceNo = index + 1;
        }
    }
}
