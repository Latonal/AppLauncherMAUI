using AppLauncherMAUI.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class SettingsViewModel : ExtendedBindableObject
{
    private int _currentThemeId = 0;
    public int CurrentThemeId { get { return _currentThemeId; } set { _currentThemeId = value; RaisePropertyChanged(() => CurrentThemeId); } }


    public SettingsViewModel()
    {
        
    }

    public void OnThemePickerIndexChanged(int id)
    {
        Debug.WriteLine("reaching this code");
        if (id == _currentThemeId || id == -1) return;

        Debug.WriteLine("reaching this code too");

        ThemeHandler.ChangeTheme(id);
        CurrentThemeId = id;
    }
}
