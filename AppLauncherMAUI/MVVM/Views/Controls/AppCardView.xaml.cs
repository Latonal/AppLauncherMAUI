using AppLauncherMAUI.Utilities;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppCardView : ContentView
{
    public static readonly BindableProperty AppCardNameProperty = BindableProperty.Create(nameof(AppCardName), typeof(string), typeof(AppCardView), string.Empty);
    public static readonly BindableProperty AppCardIdProperty = BindableProperty.Create(nameof(AppCardId), typeof(int), typeof(AppCardView), 0);



    public string AppCardName
    {
        get => (string)GetValue(AppCardNameProperty);
        set => SetValue(AppCardNameProperty, value);
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

    //public void OnPointerEntered(object sender, System.EventArgs e)
    //{

    //}

    //public void OnPointerExited(object sender, System.EventArgs e)
    //{

    //}
}