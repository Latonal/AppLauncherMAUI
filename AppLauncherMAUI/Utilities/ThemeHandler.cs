using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppLauncherMAUI.Resources.Styles;

namespace AppLauncherMAUI.Utilities;

static class ThemeHandler
{
    public static void SetSavedTheme()
    {
        int savedTheme = Preferences.Get("AppTheme", 0);

        ChangeTheme(savedTheme);
    }

    public static void ChangeTheme(int themeCode)
    {
        if (Application.Current is not null)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();

                switch (themeCode)
                {
                    case 1:
                        mergedDictionaries.Add(new LightTheme());
                        Preferences.Set("AppTheme", 1);
                        break;
                    case 2:
                        mergedDictionaries.Add(new DarkTheme());
                        Preferences.Set("AppTheme", 2);
                        break;
                    case 0:
                    default: // apply style depending of preference in system (only light/dark mode)
                        mergedDictionaries.Add(Application.Current.RequestedTheme == AppTheme.Light ? new LightTheme() : new DarkTheme());
                        Preferences.Set("AppTheme", 0);
                        break;
                }

                mergedDictionaries.Add(new AppStyles());
            }
        }
    }
}
