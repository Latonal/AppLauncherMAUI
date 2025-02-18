using AppLauncherMAUI.MVVM.ViewModels;

namespace AppLauncherMAUI.MVVM.Views;

public partial class SingleAppView : ContentView
{
    public SingleAppView(int appId)
	{
		InitializeComponent();

		BindingContext = new SingleAppViewModel(appId);
	}
}