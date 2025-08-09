using AppLauncherMAUI.MVVM.Models;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace AppLauncherMAUI.Utilities;

internal static class Common
{
    readonly static string defaultLanguage = "en";

    public enum SupportedLanguages
    {
        en,
        fr
    }

    public static string GetUserLanguage(bool useLong = false)
    {
        CultureInfo currentCulture = CultureInfo.CurrentUICulture;
        return useLong ? currentCulture.Name : currentCulture.TwoLetterISOLanguageName;
    }

    public static string GetSupportedUserLanguage(bool useLong = false)
    {
        string curr = GetUserLanguage(useLong: useLong);
        return Enum.IsDefined(typeof(SupportedLanguages), curr) ? curr : defaultLanguage;
    }

    public static void OpenFolder(string path)
    {
        if (!Directory.Exists(path)) return;

        if (OperatingSystem.IsWindows())
            Process.Start("explorer", path);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", path);
    }

    public static string GetTranslatedJsonText(LanguagesModel texts)
    {
        string lang =  ToUpperFirstLetter(GetSupportedUserLanguage());
        PropertyInfo property = typeof(LanguagesModel).GetProperty(lang) ?? throw new Exception($"(Common) Property '{lang}' could not be found. Is the language supported in LanguagesModel and is the defaultLanguage correctly assigned?");
        if (property.GetValue(texts) is string text)
            return text;

        throw new Exception($"(Common) Property '{lang}' could not be found. Is the language supported in the json?");
    }

    public static string ToUpperFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        return char.ToUpper(str[0]) + str[1..];
    }

    public static long GetCurrentUnixTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static bool CheckValidUri(string url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);

        //other:
        //return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    public static string GetUriHost(string url)
    {
        Uri uri = CheckValidUri(url) ? new Uri(url) : throw new Exception($"[DownloadHandler] The given url ({url}) is not correct.");
        return uri.Host;
    }
}
