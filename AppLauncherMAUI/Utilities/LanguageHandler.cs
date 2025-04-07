using System.Globalization;

namespace AppLauncherMAUI.Utilities;

static class LanguageHandler
{

    public static void SetSavedLanguage()
    {
        string savedLang = Preferences.Get("AppLanguage", "en");

        ChangeLanguage(savedLang);
    }

    public static void SaveLanguage(string langCode)
    {
        Preferences.Set("AppLanguage", langCode);

        ChangeLanguage(langCode);
    }

    public static void ChangeLanguage(string langCode)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture(langCode);
    }
}
