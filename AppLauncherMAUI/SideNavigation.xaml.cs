namespace AppLauncherMAUI.MVVM.Views;

public partial class SideNavigation : ContentPage
{
    public SideNavigation()
    {
        InitializeComponent();

        //PageContainer.Content = new MainPage();
        //LoadPage(new MainPage());
        GoToHomeView(null, null);
    }

    public void LoadPage(View view)
    {
        if (view == null) return;

        if (PageContainer.Content is View oldView)
        {
            oldView.BindingContext = null;
            oldView.Handler?.DisconnectHandler();
        }

        PageContainer.Content = view;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void GoToHomeView(object? sender, EventArgs? e)
    {
        LoadPage(new HomeView());
    }

    private void GoToSettingsView(object sender, EventArgs e)
    {
        LoadPage(new SettingsView());
    }
}