using AppLauncherMAUI.MVVM.ViewModels;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.Views.Controls;

public partial class AppDownloadButton : ContentView
{
    private readonly List<VisualElement> uiElements = [];

    public AppDownloadButton()
    {
        InitializeComponent();

        uiElements.AddRange([
            stackLayout,
            defaultDownloadButton,
            launchButton,
            cancelButton,
        ]);
    }

    public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(nameof(AppDownloadButtonStates), typeof(AppDownloadButtonStates), typeof(AppDownloadButton), AppDownloadButtonStates.Loading, propertyChanged: OnButtonStateChanged);
    public static readonly BindableProperty ButtonCommandProperty = BindableProperty.Create(nameof(ButtonCommand), typeof(ICommand), typeof(AppDownloadButton));
    public static readonly BindableProperty ProgressValueProperty = BindableProperty.Create(nameof(ProgressValue), typeof(double), typeof(AppDownloadButton), 0.0, propertyChanged: OnProgressChanged);

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

    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    private static void OnButtonStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var button = (AppDownloadButton)bindable;
        button.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        string state = ButtonState.ToString();
        foreach (VisualElement uiElement in uiElements)
        {
            VisualStateManager.GoToState(uiElement, state);
        }
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

    public static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AppDownloadButton view && newValue is double progress)
        {
            view.progressText.Text = $"{Math.Round(progress * 100)}%";
        }
    }
} 

public enum AppDownloadButtonStates
{
    Loading,
    Disabled,
    CanInstall,
    Downloading,
    Playable,
    Update,
    Delete,
    Active,
    Error,
}

public enum AppDownloadButtonCommand
{
    Download,
    Launch,
    Cancel,
    Delete,
    OpenFolder,
    Stop,
    Next,
    NoUse,
}