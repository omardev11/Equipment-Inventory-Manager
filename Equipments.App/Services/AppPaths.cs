namespace Equipments.App.Services;

public static class AppPaths
{
    public static string DataDirectory
    {
        get
        {
            var path = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string DatabasePath => Path.Combine(DataDirectory, "equipments.json");
    public static string StfRequestsPath => Path.Combine(DataDirectory, "stf-requests.json");
    public static string MaterialMovementsPath => Path.Combine(DataDirectory, "material-movements.json");

    public static string SupabaseSettingsPath => Path.Combine(DataDirectory, "supabase-settings.json");

    public static string ExportsDirectory
    {
        get
        {
            var path = Path.Combine(AppContext.BaseDirectory, "exports");
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
