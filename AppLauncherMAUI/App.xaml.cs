namespace AppLauncherMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            SetTheme();

            MainPage = new AppShell();
        }

        private void SetTheme()
        {
            int savedTheme = Preferences.Get("AppTheme", 0);
            UserAppTheme = savedTheme switch
            {
                1 => AppTheme.Light,
                2 => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }
    }
}
