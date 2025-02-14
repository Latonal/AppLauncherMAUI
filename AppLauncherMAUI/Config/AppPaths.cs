namespace AppLauncherMAUI.Config
{
    internal static class AppPaths
    {
        public static string DataDirectory => FileSystem.AppDataDirectory;
        public static string CacheDirectory => FileSystem.CacheDirectory;

        public static string AppsDataJsonName => "apps_data.json";

        public static string AppDataJson => Path.Combine(DataDirectory, AppsDataJsonName);
    }
}
