using System.Text.Json;

namespace AppLauncherMAUI.Utilities;

internal class JsonFileManager
{
    public static async Task<T> ReadDataAsync<T>(string filename)
    {
        var json = await LoadMauiAsset(filename);
        return JsonSerializer.Deserialize<T>(json) ?? throw new Exception("Something happened while deserializing the Json. Json might be null.");
    }

    public static async Task<string> LoadMauiAsset(string filename)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            throw new Exception("Something wrong happened while loading the MauiAsset: " + filename + "\n" + e);
        }
    }
}