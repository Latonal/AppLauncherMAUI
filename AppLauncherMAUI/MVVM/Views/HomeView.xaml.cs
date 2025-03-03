using AppLauncherMAUI.MVVM.ViewModels;

namespace AppLauncherMAUI.MVVM.Views;

public partial class HomeView : ContentView
{
    public HomeView()
    {
        InitializeComponent();

        BindingContext = new HomeViewModel();
    }

    public void OnCollectionViewSizeChanged(object sender, EventArgs e)
    {
        if (sender is CollectionView cv)
        {
            double maxWidth = cv.Width;

            if (cv.ItemsLayout is GridItemsLayout grid)
            {
                // size of card + margin = 220 + 20
                grid.Span = (int)Math.Floor(maxWidth / 240);
            }
        }
    }
}
