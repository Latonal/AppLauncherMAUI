using AppLauncherMAUI.MVVM.ViewModels;

namespace AppLauncherMAUI.MVVM.Views;

public partial class HomeView : ContentView
{
    public HomeView()
    {
        InitializeComponent();

        BindingContext = new HomeViewModel();
    }
}
