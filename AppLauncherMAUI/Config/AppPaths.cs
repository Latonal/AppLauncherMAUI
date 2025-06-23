namespace AppLauncherMAUI.Config
{
    internal static class AppPaths
    {
        public static string LocalDataDirectory => FileSystem.AppDataDirectory;
        public static string CacheDirectory => FileSystem.CacheDirectory;

        public static string AppsDataJsonName => "apps_data.json";
        public static string AppDataJson => Path.Combine(LocalDataDirectory, AppsDataJsonName);
        public static Func<string, string> ZipPath = name => Path.Combine(CacheDirectory, name, "data.zip");
        public static Func<string, string> DownloadedAppPath = name => Path.Combine(LocalDataDirectory, "apps", name);
    }
}
