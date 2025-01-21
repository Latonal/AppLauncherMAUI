using AppLauncherMAUI.Resources.Styles;
using System.Diagnostics;

namespace AppLauncherMAUI.MVVM.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        ThemePicker.SelectedIndex = Preferences.Get("AppTheme", 0);
    }

    private void OnThemePickerChanged(object sender, EventArgs e)
    {
        if (ThemePicker.SelectedIndex == -1)
            return;

        ChangeTheme(ThemePicker.SelectedIndex);
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