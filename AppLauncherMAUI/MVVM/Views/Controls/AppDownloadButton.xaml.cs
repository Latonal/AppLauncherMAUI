using AppLauncherMAUI.MVVM.ViewModels;
using System.Diagnostics;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppDownloadButton : Button
{
    public AppDownloadButton()
    {
        InitializeComponent();

        Clicked += (s, e) =>
        {
            if (ButtonCommand != null && ButtonCommand.CanExecute(null))
            {
                ButtonCommand.Execute(null);
            }
        };
    }

    public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(nameof(AppDownloadButtonState), typeof(AppDownloadButtonState), typeof(AppDownloadButton), AppDownloadButtonState.Disabled, propertyChanged: OnButtonStateChanged);
    public static readonly BindableProperty ButtonCommandProperty = BindableProperty.Create(nameof(ButtonCommand), typeof(ICommand), typeof(AppDownloadButton));

    public AppDownloadButtonState ButtonState
    {
        get => (AppDownloadButtonState)GetValue(ButtonStateProperty);
        set => SetValue(ButtonStateProperty, value);
    }

    public ICommand ButtonCommand
    {
        get => (ICommand)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    private static void OnButtonStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var button = (AppDownloadButton)bindable;
        button.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        string state = ButtonState.ToString();
        VisualStateManager.GoToState(this, state);
    }
}

public enum AppDownloadButtonState
{
    Disabled,
    ToDownload,
    Downloading,
    Playable,
    UpdateAvailable,
    Error
}
