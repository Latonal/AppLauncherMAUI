using AppLauncherMAUI.MVVM.ViewModels;

namespace AppLauncherMAUI.MVVM.Views;

public partial class SingleAppView : ContentView
{
	private const double TargetAspectRatio = 35 / 9;

    public SingleAppView(int appId)
	{
		InitializeComponent();

		BindingContext = new SingleAppViewModel(appId);
	}

	private void OnBannerSizeChanged(object sender, EventArgs e)
	{
		if (sender is Border border)
		{
			double width = border.Width;
			double height = width / TargetAspectRatio;

			border.HeightRequest = height;
		}
    }

	private async void OpenBrowser(object sender, EventArgs args)
	{
		try
		{
			if (sender is Button button && button.CommandParameter is String url)
			{
				Uri uri = new(url);
				await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
			}
        }
		catch (Exception ex) 
		{
			throw new Exception($"[SingleAppView] The following error happened when attempting to open the browser: {ex.Message}");
		}
	}
}