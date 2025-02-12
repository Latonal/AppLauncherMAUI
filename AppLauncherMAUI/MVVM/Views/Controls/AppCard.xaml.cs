using AppLauncherMAUI.Utilities;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppCard : ContentView
{
    public static readonly BindableProperty AppCardTextProperty = BindableProperty.Create(nameof(AppCardText), typeof(string), typeof(AppCard), string.Empty);
    public static readonly BindableProperty AppCardIdProperty = BindableProperty.Create(nameof(AppCardId), typeof(int), typeof(AppCard), 0);

    public string AppCardText
    {
        get => (string)GetValue(AppCardTextProperty);
        set => SetValue(AppCardTextProperty, value);
    }

    public int AppCardId
    {
        get => (int)GetValue(AppCardIdProperty);
        set => SetValue(AppCardIdProperty, value);
    }

    public AppCard()
    {
        InitializeComponent();
    }

    public void OnBorderTapped(object sender, System.EventArgs e)
    {
        ViewManager.ChangeActiveView(new SingleAppView(AppCardId));
    }
}