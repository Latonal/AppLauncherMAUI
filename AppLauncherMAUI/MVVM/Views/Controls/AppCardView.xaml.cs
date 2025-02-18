using AppLauncherMAUI.Utilities;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppCardView : ContentView
{
    public static readonly BindableProperty AppCardTextProperty = BindableProperty.Create(nameof(AppCardText), typeof(string), typeof(AppCardView), string.Empty);
    public static readonly BindableProperty AppCardIdProperty = BindableProperty.Create(nameof(AppCardId), typeof(int), typeof(AppCardView), 0);



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