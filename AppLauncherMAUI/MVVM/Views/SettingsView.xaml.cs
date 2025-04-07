using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.ViewModels;

namespace AppLauncherMAUI.MVVM.Views;

public partial class SettingsView : ContentView
{
    private readonly bool init = false;

    public SettingsView()
    {
        InitializeComponent();

        BindingContext = new SettingsViewModel();

        ThemePicker.SelectedIndex = Preferences.Get("AppTheme", 0);
        init = true;
    }

    private void OnThemePickerChanged(object sender, EventArgs e)
    {
        if (ThemePicker.SelectedIndex == -1 || init == false)
            return;

        if (BindingContext is SettingsViewModel viewModel) {
            viewModel.OnThemePickerIndexChanged(ThemePicker.SelectedIndex);
        }
    }

    private void OnLanguagePickerChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex == -1 || init == false)
            return;

        if (BindingContext is SettingsViewModel viewModel)
        {
            ItemStringIdPickerModel si = (ItemStringIdPickerModel)LanguagePicker.SelectedItem;
            viewModel.OnLanguagePickerChanged(si.Id);
            PlsRestart.IsVisible = true;
        }
    }
}