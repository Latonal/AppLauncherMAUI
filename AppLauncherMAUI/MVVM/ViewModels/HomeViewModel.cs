using AppLauncherMAUI.Utilities;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class HomeViewModel : ExtendedBindableObject
{
    private int _clicksReceived = 0;
    public int ClicksReceived { get {  return _clicksReceived; } set { _clicksReceived = value; RaisePropertyChanged(() => ClicksReceived); } }
    public ICommand ActionClickedCommand { get; set; }


    public HomeViewModel()
    {
        ActionClickedCommand ??= new Command(OnActionClicked);
    }

    private void OnActionClicked(object obj)
    {
        ClicksReceived++;
    }
}
