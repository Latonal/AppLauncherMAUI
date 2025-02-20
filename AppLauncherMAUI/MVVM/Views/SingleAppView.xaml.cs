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
}