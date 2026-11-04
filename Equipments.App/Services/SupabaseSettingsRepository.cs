using System.Text.Json;

namespace Equipments.App.Services;

public sealed class SupabaseSettingsRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public SupabaseSettingsRepository(string filePath)
    {
        _filePath = filePath;
    }

    public SupabaseSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            return new SupabaseSettings();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<SupabaseSettings>(json, JsonOptions) ?? new SupabaseSettings();
        }
        catch
        {
            return new SupabaseSettings();
        }
    }

    public void Save(SupabaseSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
