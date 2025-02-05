using System.Diagnostics;
using AppLauncherMAUI.MVVM.ViewModels;

namespace AppLauncherMAUI.MVVM.Views;

public partial class HomeView : ContentView
{
    //private int count = 0;
    //public string? Message { get; private set; }

    public HomeView()
    {
        InitializeComponent();

        BindingContext = new HomeViewModel();
        //SetMessage(number);
    }

    //private void SetMessage(int number)
    //{
    //    Message = "Came on this page " + number + " time.";
    //}

    private static void GoToSingleAppView(object sender, EventArgs e)
    {
        //SideNavigation sideNavigation = new();

        //Debug.WriteLine(sender.CommandParameter);
        //sideNavigation.LoadPage(new SingleAppView(id));
    }
}
