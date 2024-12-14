namespace AppLauncherMAUI.MVVM.Views;

public partial class SettingsView : ContentPage
{
    public SettingsView()
    {
        InitializeComponent();

        ThemePicker.SelectedIndex = GetCurrentThemeIndex();
    }

    private void OnThemePickerChanged(object sender, EventArgs e)
    {
        if (ThemePicker.SelectedIndex == -1)
            return;

        switch (ThemePicker.SelectedIndex)
        {
            case 1:
                if (Application.Current is not null)
                    Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case 2:
                if (Application.Current is not null)
                    Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            case 0:
            default:
                if (Application.Current is not null)
                    Application.Current.UserAppTheme = AppTheme.Unspecified;
                break;
        }

        Preferences.Set("AppTheme", ThemePicker.SelectedIndex);
    }

    private static int GetCurrentThemeIndex()
    {
        AppTheme currentTheme = Application.Current is not null ? Application.Current.RequestedTheme : AppTheme.Unspecified;

        return currentTheme switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark => 2,
            _ => 0, // Default - System
        };
    }
}