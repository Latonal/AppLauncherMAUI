using System.Globalization;

namespace AppLauncherMAUI.Utilities;

internal static class Common
{
    public static string GetUserLanguage(bool useLong = false)
    {
        CultureInfo currentCulture = CultureInfo.CurrentCulture;
        return useLong ? currentCulture.Name : currentCulture.TwoLetterISOLanguageName;
    }
}
