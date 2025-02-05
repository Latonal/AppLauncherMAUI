namespace AppLauncherMAUI.MVVM.Views;

public partial class SingleAppView : ContentView
{
    public int? AppId { get; private set; }

    public SingleAppView(int appId)
	{
		InitializeComponent();
		AppId = appId;
		GeneratePage();
	}

	private void GeneratePage()
	{
		TextAppId.Text = AppId.ToString();
	}
}