using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Timeout = TimeSpan.FromSeconds(30),
        };

        return httpClient;
    }

    public static async Task<string?> GetHeaderAsync(string url)
    {
        if (url == null) return null;
        _httpClient ??= GetNewHttpClient();

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentType?.MediaType;
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
}
