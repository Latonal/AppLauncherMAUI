using System.Diagnostics;

namespace AppLauncherMAUI.Utilities;

public class ExternalApplicationManager
{
    private Process? _process;

    public ExternalApplicationManager() { }

    public enum AllowedContentType
    {
        Zip,
        Executable,
        OctetStream,
        Unknown
    }

    public static AllowedContentType GetAllowedContentType(string headerValue)
    {
        return headerValue switch
        {
            "application/zip" or "application/x-zip" or "application/x-zip-compressed" => AllowedContentType.Zip,
            "application/vnd.microsoft.portable-executable" => AllowedContentType.Executable, /* might not be enough */
            "application/octet-stream" => AllowedContentType.OctetStream,

            _ => AllowedContentType.Unknown,
        };
    }

    /// <summary>
    /// Start the application
    /// </summary>
    /// <param name="appPath"></param>
    /// <param name="arguments"></param>
    public void StartApplication(string appPath, string? arguments = null)
    {
        if (_process != null && !_process.HasExited)
        {
            Console.WriteLine("(ExternalApplicationManager) Process is already running.");
            return;
        }

        bool isExe = IsFileExecutable(appPath);

        ProcessStartInfo infos = new()
        {
            FileName = appPath,
            Arguments = arguments ?? "",
            UseShellExecute = !isExe, /* false for executable, true for others */
            CreateNoWindow = false,
        };

        try
        {
            if (!OperatingSystem.IsIOS())
                _process = Process.Start(infos);
            else
                Console.Error.WriteLine("Launching an application is not supported by your OS.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"(ExternalApplicationManager) An error happened: {ex.Message}");
        }
    }

    public static bool IsFileExecutable(string filePath)
    {
        var signature = "MZ"u8.ToArray(); // "MZ" is for executable files
        
        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[2];
        fs.ReadExactly(buffer);

        return signature.SequenceEqual(buffer);
    }

    public bool IsRunning()
    {
        return _process != null && !_process.HasExited;
    }

    public void StopApplication()
    {
        if (_process != null && !_process.HasExited)
        {

            if (!OperatingSystem.IsIOS())
                _process.Kill(true);
            else
                _process.CloseMainWindow();

            _process.WaitForExit();
        }
    }
}
