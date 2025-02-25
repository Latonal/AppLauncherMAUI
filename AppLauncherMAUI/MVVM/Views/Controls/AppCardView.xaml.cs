using AppLauncherMAUI.Utilities;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppCardView : ContentView
{
    public static readonly BindableProperty AppCardMiniBannerProperty = BindableProperty.Create(nameof(AppCardMiniBanner), typeof(string), typeof(AppCardView), string.Empty);
    public static readonly BindableProperty AppCardIdProperty = BindableProperty.Create(nameof(AppCardId), typeof(int), typeof(AppCardView), 0);



    public string AppCardMiniBanner
    {
        get => (string)GetValue(AppCardMiniBannerProperty);
        set => SetValue(AppCardMiniBannerProperty, value);
    }

    public int AppCardId
    {
        get => (int)GetValue(AppCardIdProperty);
        set => SetValue(AppCardIdProperty, value);
    }

    public AppCardView()
    {
        InitializeComponent();
    }

    public void OnBorderTapped(object sender, System.EventArgs e)
    {
        ViewManager.ChangeActiveView(new SingleAppView(AppCardId));
    }

    public async void OnPointerEntered(object sender, System.EventArgs e)
    {
        await image.ScaleTo(1.1, 250);
    }

    public async void OnPointerExited(object sender, System.EventArgs e)
    {
        await image.ScaleTo(1, 250);
    }
}