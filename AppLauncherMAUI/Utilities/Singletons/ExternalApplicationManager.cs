using System.Diagnostics;

namespace AppLauncherMAUI.Utilities.Singletons;

public sealed class ExternalApplicationManager
{
    private static readonly Lazy<ExternalApplicationManager> _instance = new(() => new ExternalApplicationManager());
    public static readonly ExternalApplicationManager Instance = _instance.Value;
    public static readonly Lock padlock = new();

    private readonly Dictionary<int, (Process Process, string Signature)> _runningApps = [];
    private const string appSignature = "randomString_zjSjgslBwbCNrXGJBLXI";

    public event Action<int, string>? ProcessTerminated;

    public bool StartApplication(string appPath, int appId, string? arguments = null)
    {
        if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
        {
            throw new FileNotFoundException("[ExternalApplicationManager] Given file path is invalid.", appPath);
        }

        if (OperatingSystem.IsIOS())
        {
            Console.Error.WriteLine("[ExternalApplicationManager] Launching an application is not supported by your OS.");
            return false;
        }

        try
        {
            bool isExe = IsFileExecutable(appPath);

            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = arguments ?? "",
                    UseShellExecute = !isExe, /* false for executable, true for others */
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, e) =>
            {
                if (sender is Process exitedProcess)
                    OnProcessTerminated(exitedProcess.Id);
            };

            if (!process.Start())
                return false;

            string signature = GenerateSignature(process.Id);

            lock (padlock)
                _runningApps[appId] = (process, signature);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExternalApplicationManager] Error while launching the applicaiton: {ex.Message}");
            return false;
        }
    }

    public bool IsApplicationRunning(int appId)
    {
        lock (padlock)
        {
            if (_runningApps.TryGetValue(appId, out (Process Process, string Signature) processInfo))
            {
                try
                {
                    if (processInfo.Process.HasExited)
                    {
                        _runningApps.Remove(appId);
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    _runningApps.Remove(appId);
                    return false;
                }
            }
            return false;
        }
    }

    private static string GenerateSignature(int processId)
    {
        return $"{processId}_{appSignature}_{DateTime.Now.Ticks}";
    }

    private static bool IsFileExecutable(string filePath)
    {
        try
        {
            var signature = "MZ"u8.ToArray(); // "MZ" is for executable files

            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[2];
            fs.ReadExactly(buffer);

            return signature.SequenceEqual(buffer);
        }
        catch
        {
            return false;
        }
    }

    public bool KillApplication(int appId)
    {
        lock (padlock)
        {
            if (_runningApps.TryGetValue(appId, out (Process Process, string Signature) processInfo))
            {
                try
                {
                    if (!processInfo.Process.HasExited)
                    {
                        if (OperatingSystem.IsIOS())
                            processInfo.Process.CloseMainWindow();
                        else
                            processInfo.Process.Kill();

                        processInfo.Process.WaitForExit();
                        _runningApps.Remove(appId);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExternalApplicationManager] Error while terminating the process {appId}: {ex.Message}");
                }
            }

            return false;
        }
    }

    private void OnProcessTerminated(int processId)
    {
        lock (padlock)
        {
            foreach (var app in _runningApps)
            {
                if (app.Value.Process.Id == processId)
                {
                    _runningApps.Remove(app.Key);
                    ProcessTerminated?.Invoke(app.Key, app.Value.Signature);
                    Debug.WriteLine($"Process with id {processId} has been stopped");
                    return;
                }
            }
        }
    }

    public static AllowedContentType GetAllowedContentType(string headerValue)
    {
        return headerValue switch
        {
            "application/zip" or "application/x-zip" or "application/x-zip-compressed" => AllowedContentType.Zip,
            "application/json" => AllowedContentType.Json,
            "application/vnd.microsoft.portable-executable" => AllowedContentType.Executable, /* might not be enough */
            "application/octet-stream" => AllowedContentType.OctetStream,

            _ => AllowedContentType.Unknown,
        };
    }

    public static AllowedContentType GetAppAllowedContentType(string headerValue)
    {
        return headerValue switch
        {
            "application/zip" or "application/x-zip" or "application/x-zip-compressed" => AllowedContentType.Zip,
            "application/json" => AllowedContentType.Json,
            "application/vnd.microsoft.portable-executable" => AllowedContentType.Executable, /* might not be enough */
            "application/octet-stream" => AllowedContentType.OctetStream,

            _ => AllowedContentType.Unknown,
        };
    }

    public static AllowedContentType GetVersionAllowedContentType(string headerValue)
    {
        return headerValue switch
        {
            "text/plain" => AllowedContentType.Text,
            "application/json" => AllowedContentType.Json,

            _ => AllowedContentType.Unknown,
        };
    }

    public enum AllowedContentType
    {
        Zip,
        Json,
        Executable,
        OctetStream,
        Text,
        Unknown
    }
}
