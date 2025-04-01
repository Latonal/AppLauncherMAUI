using AppLauncherMAUI.MVVM.ViewModels;
using System.Diagnostics;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppDownloadButton : ContentView
{
    public AppDownloadButton()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(nameof(AppDownloadButtonStates), typeof(AppDownloadButtonStates), typeof(AppDownloadButton), AppDownloadButtonStates.Disabled, propertyChanged: OnButtonStateChanged);
    public static readonly BindableProperty ButtonCommandProperty = BindableProperty.Create(nameof(ButtonCommand), typeof(ICommand), typeof(AppDownloadButton));

    public AppDownloadButtonStates ButtonState
    {
        get => (AppDownloadButtonStates)GetValue(ButtonStateProperty);
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
        Debug.WriteLine("Updating visual state to: " + ButtonState.ToString());
        string state = ButtonState.ToString();
        VisualStateManager.GoToState(stackLayout, state);
        VisualStateManager.GoToState(defaultDownloadButton, state);
        VisualStateManager.GoToState(cancelButton, state);
    }

    private void ButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is AppDownloadButtonCommand cmd)
        {
            ButtonCommand.Execute(cmd);
        }
        else if (ButtonCommand != null && ButtonCommand.CanExecute(null))
        {
            ButtonCommand.Execute(null);
        }
    }
}

public enum AppDownloadButtonStates
{
    Disabled,
    Install,
    Downloading,
    Playable,
    Update,
    Delete,
    Repair,
    Error
}

public enum AppDownloadButtonCommand
{
    Launch,
    Cancel,
    Delete,
    Next
}