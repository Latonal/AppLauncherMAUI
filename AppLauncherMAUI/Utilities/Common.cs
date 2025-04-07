using AppLauncherMAUI.MVVM.Models;
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
}
