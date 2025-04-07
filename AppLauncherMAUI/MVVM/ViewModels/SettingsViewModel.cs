using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.Utilities;
using System.Collections.ObjectModel;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class SettingsViewModel : ExtendedBindableObject
{
    private int _currentThemeId = 0;
    public int CurrentThemeId { get { return _currentThemeId; } set { _currentThemeId = value; } }
    private ItemStringIdPickerModel _currentLanguageId;
    public ItemStringIdPickerModel CurrentLanguageId { get { return _currentLanguageId; } set { _currentLanguageId ??= value; } }

    public ObservableCollection<ItemStringIdPickerModel> LanguageItems { get; private set; }

    public SettingsViewModel()
    {
        LanguageItems =
        [
            new() { Id = "en", Name = "English" },
            new() { Id = "fr", Name = "Français" },
        ];

        string savedLangId = Preferences.Get("AppLanguage", "en");
        _currentLanguageId = CurrentLanguageId = LanguageItems.FirstOrDefault(lang => lang.Id == savedLangId) ?? LanguageItems.First();
    }

    public void OnThemePickerIndexChanged(int id)
    {
        if (id == _currentThemeId || id == -1) return;

        ThemeHandler.ChangeTheme(id);
        CurrentThemeId = id;
    }

    public void OnLanguagePickerChanged(string id)
    {
        if (id == _currentLanguageId.Id || id == string.Empty) return;

        LanguageHandler.SaveLanguage(id);
        CurrentLanguageId = LanguageItems.FirstOrDefault(lang => lang.Id == id) ?? LanguageItems.First();
    }
}
