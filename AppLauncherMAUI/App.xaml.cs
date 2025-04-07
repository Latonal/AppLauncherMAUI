using AppLauncherMAUI.MVVM.Views;
using AppLauncherMAUI.Utilities;

namespace AppLauncherMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            RequestedThemeChanged += (s, a) => ThemeHandler.SetSavedTheme();
            ThemeHandler.SetSavedTheme();
            LanguageHandler.SetSavedLanguage();

            // Debug
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                Console.WriteLine($"Unhandled Exception: {e.ExceptionObject}");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new SideNavigation());
        }
    }
}
