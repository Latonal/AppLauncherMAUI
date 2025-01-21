using AppLauncherMAUI.MVVM.Views;

namespace AppLauncherMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            SetTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private static void SetTheme()
        {
            int savedTheme = Preferences.Get("AppTheme", 0);

            SettingsPage.ChangeTheme(savedTheme);
        }
    }
}
