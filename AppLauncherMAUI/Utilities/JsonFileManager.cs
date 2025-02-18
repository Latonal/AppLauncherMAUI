using System.Text.Json;

namespace AppLauncherMAUI.Utilities;

internal class JsonFileManager
{
    public static async Task<T> ReadDataAsync<T>(string filename)
    {
        var json = await LoadMauiAsset(filename);
        return JsonSerializer.Deserialize<T>(json) ?? throw new Exception("(JsonFileManager) Something happened while deserializing the Json. Json might be null.");
    }

    public static async Task<T> ReadSingleDataAsync<T>(string filename, string nameOfValueToMatchWith, object valueToMatchWith)
    {
        var property = typeof(T).GetProperty(nameOfValueToMatchWith) ?? throw new Exception($"(JsonFileManager) Property {nameOfValueToMatchWith} could not be found on type {typeof(T).Name}.");
        List<T> json = await ReadDataAsync<List<T>>(filename) ?? throw new Exception();

        foreach (T obj in json)
        {
            var value = property.GetValue(obj);
            if (value != null && value.Equals(valueToMatchWith))
            {
                return obj;
            }
        }

        throw new Exception($"(JsonFileManager) No corresponding object with parameters '{nameOfValueToMatchWith}:{valueToMatchWith}'.");
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
            throw new Exception($"(JsonFileManager) Something wrong happened while loading the MauiAsset: {filename}\n{e}");
        }
    }
}