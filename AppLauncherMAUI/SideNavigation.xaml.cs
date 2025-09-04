using AppLauncherMAUI.Utilities.Singletons;

namespace AppLauncherMAUI.MVVM.Views;

public partial class SideNavigation : ContentPage
{
    public SideNavigation()
    {
        InitializeComponent();
        _ = new ViewManager(PageContainer);

        ViewManager.ChangeActiveView(new HomeView());
    }

    private void GoToHomeView(object sender, EventArgs e)
    {
        ViewManager.ChangeActiveView(new HomeView());
    }

    private void GoToSettingsView(object sender, EventArgs e)
    {
        ViewManager.ChangeActiveView(new SettingsView());
    }
}