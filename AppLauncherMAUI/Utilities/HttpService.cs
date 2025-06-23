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

    public HttpService() { }

    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static HttpClient Client
    {
        get => _httpClient ??= new HttpClient();
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
}
