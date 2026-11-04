using System.Text.Json;
using Equipments.App.Models;

namespace Equipments.App.Services;

public sealed class StfRequestRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public StfRequestRepository(string filePath)
    {
        _filePath = filePath;
    }

    public List<StfRequestItem> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        var items = JsonSerializer.Deserialize<List<StfRequestItem>>(json, JsonOptions) ?? [];
        return Normalize(items);
    }

    public void Save(IEnumerable<StfRequestItem> items)
    {
        var normalized = Normalize(items).OrderBy(x => x.RowNo).ToList();
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllText(_filePath, json);
    }

    public static List<StfRequestItem> Normalize(IEnumerable<StfRequestItem> items)
    {
        var normalized = items
            .Where(x => x is not null)
            .Select(x =>
            {
                x.Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id;
                x.StfNo = (x.StfNo ?? string.Empty).Trim();
                x.RequestedBy = (x.RequestedBy ?? string.Empty).Trim();
                x.Department = (x.Department ?? string.Empty).Trim();
                x.ProductName = (x.ProductName ?? string.Empty).Trim();
                x.Unit = string.IsNullOrWhiteSpace(x.Unit) ? "Adet" : x.Unit.Trim();
                x.Description = (x.Description ?? string.Empty).Trim();
                x.Status = (x.Status ?? string.Empty).Trim();
                x.RequestedQuantity = x.RequestedQuantity <= 0 ? 1 : x.RequestedQuantity;
                x.ReceivedQuantity = Math.Max(0, x.ReceivedQuantity);
                x.Status = ComputeStatus(x);
                return x;
            })
            .ToList();

        Resequence(normalized);
        return normalized;
    }

    public static void Resequence(IList<StfRequestItem> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            items[i].RowNo = i + 1;
        }
    }

    public static string ComputeStatus(StfRequestItem item)
    {
        if (item.ReceivedQuantity <= 0)
        {
            return "Bekliyor";
        }

        if (item.ReceivedQuantity < item.RequestedQuantity)
        {
            return "Kısmi Geldi";
        }

        return "Alındı";
    }
}
