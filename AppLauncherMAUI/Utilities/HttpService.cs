using AppLauncherMAUI.Config;
using System.Net.Http.Headers;

namespace AppLauncherMAUI.Utilities;

public sealed class HttpService
{
    private static HttpService? instance;
    public static readonly Lock padlock = new();
    private static HttpClient? _httpClient;

    public HttpService()
    {
        _httpClient = GetNewHttpClient();
    }

    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static HttpClient Client
    {
        get => _httpClient ??= GetNewHttpClient();
        set => _httpClient = value;
    }

    public static HttpService Instance
    {
        get
        {
            lock (padlock)
            {
                instance ??= new HttpService();
                return instance;
            }
        }
    }

    private static HttpClient GetNewHttpClient()
    {
        HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromMinutes(30),
        };

        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"{AppConfig.AppCommName}", $"{AppConfig.AppVersion}"));

        return httpClient;
    }

    public static async Task<HttpResponseMessage?> MakeCall(string url)
    {
        if (url == null) return null;
        _httpClient ??= GetNewHttpClient();

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"(HttpService) [HttpRequestException] {ex.Message}");
        }
        catch (IOException ex)
        {
            throw new Exception($"(HttpService) [IOException] {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"(HttpService) {ex.Message}");
        }
    }
        
    public static async Task<HttpContentHeaders?> GetContentHeaderAsync(string url)
    {
        HttpResponseMessage? response = await MakeCall(url);
        return response?.Content.Headers;
    }

    public static async Task<HttpResponseMessage?> GetFullResponseAsync(string url)
    {
        HttpResponseMessage? response = await MakeCall(url);
        return response;
    }
}
