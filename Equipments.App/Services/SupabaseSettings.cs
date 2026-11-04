namespace Equipments.App.Services;

public sealed class SupabaseSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string EquipmentTable { get; set; } = "equipments";
    public bool AutoSyncEnabled { get; set; } = true;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(PublishableKey) &&
        !string.IsNullOrWhiteSpace(EquipmentTable);

    public string GetRestTableUrl()
    {
        var normalizedBase = BaseUrl.Trim().TrimEnd('/');
        return $"{normalizedBase}/rest/v1/{EquipmentTable.Trim()}";
    }
}
