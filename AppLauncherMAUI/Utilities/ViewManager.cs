using System.Diagnostics;

namespace AppLauncherMAUI.Utilities;

// From: https://csharpindepth.com/articles/singleton

public sealed class ViewManager
{
    private static ViewManager? instance;
    private static readonly Lock padlock = new();

    private static ContentView? _contentView;

    public ViewManager() { }

    public ViewManager(ContentView container)
    {
        ContentView = container;
    }

    public static ContentView ContentView
    {
        get => _contentView ?? throw new InvalidOperationException("ViewManager is attempting to access a ContentView but none has been assigned.");
        set => _contentView = value;
    }

    public static ViewManager Instance
    {
        get {
            lock (padlock)
            {
                instance ??= new ViewManager();
                return instance;
            }
        }
    }

    public static void ChangeActiveView(View newView)
    {
        if (ContentView == null || newView == null) return;

        if (ContentView.Content is View oldView)
        {
            if (newView.ToString() == oldView.ToString()) return;

            oldView.BindingContext = null;
            oldView.Handler?.DisconnectHandler();
        }

        ContentView.Content = newView;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
